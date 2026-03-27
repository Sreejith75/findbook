using FindBook.Core.Delivery.DeliveryTaskAggregate;
using FindBook.Core.Delivery.DeliveryTaskAggregate.Specifications;
using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.LibraryInventory.BookAggregate.Specifications;
using FindBook.Core.Rental.RentalAggregate;
using FindBook.Core.Rental.RentalAggregate.Specifications;
using FindBook.Core.UserManagement.UserAccountAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate.Specifications;
using FindBook.UseCases.Libraries;

namespace FindBook.UseCases.Delivery;

public sealed record DeliveryTaskDto(
  int Id,
  int RentalId,
  int DeliveryPartnerAccountId,
  string Type,
  string Status,
  DateTimeOffset AssignedAt,
  DateTimeOffset? CompletedAt,
  AddressDto DeliveryAddress);

public sealed record ListDeliveryTasksQuery(UserAccountId? DeliveryPartnerAccountId, DeliveryTaskStatus? Status, DeliveryTaskType? Type) : IQuery<Result<IReadOnlyCollection<DeliveryTaskDto>>>;
public sealed record GetDeliveryTaskByIdQuery(DeliveryTaskId DeliveryTaskId) : IQuery<Result<DeliveryTaskDto>>;
public sealed record CreateDeliveryTaskCommand(RentalId RentalId, UserAccountId DeliveryPartnerAccountId, DeliveryTaskType Type, DateTimeOffset AssignedAt) : ICommand<Result<DeliveryTaskDto>>;
public sealed record MarkDeliveryTaskInTransitCommand(DeliveryTaskId DeliveryTaskId) : ICommand<Result<DeliveryTaskDto>>;
public sealed record CompleteDeliveryTaskCommand(DeliveryTaskId DeliveryTaskId, DateTimeOffset CompletedAt) : ICommand<Result<DeliveryTaskDto>>;
public sealed record FailDeliveryTaskCommand(DeliveryTaskId DeliveryTaskId) : ICommand<Result<DeliveryTaskDto>>;

public sealed class ListDeliveryTasksHandler(IReadRepository<DeliveryTask> repository)
  : IQueryHandler<ListDeliveryTasksQuery, Result<IReadOnlyCollection<DeliveryTaskDto>>>
{
  public async ValueTask<Result<IReadOnlyCollection<DeliveryTaskDto>>> Handle(ListDeliveryTasksQuery query, CancellationToken cancellationToken) =>
    Result.Success<IReadOnlyCollection<DeliveryTaskDto>>((await repository.ListAsync(new ListDeliveryTasksSpec(query.DeliveryPartnerAccountId, query.Status, query.Type), cancellationToken)).Select(DeliveryTaskMappings.Map).ToArray());
}

public sealed class GetDeliveryTaskByIdHandler(IReadRepository<DeliveryTask> repository)
  : IQueryHandler<GetDeliveryTaskByIdQuery, Result<DeliveryTaskDto>>
{
  public async ValueTask<Result<DeliveryTaskDto>> Handle(GetDeliveryTaskByIdQuery query, CancellationToken cancellationToken)
  {
    var task = await repository.FirstOrDefaultAsync(new DeliveryTaskByIdSpec(query.DeliveryTaskId), cancellationToken);
    return task is null ? Result.NotFound() : Result.Success(DeliveryTaskMappings.Map(task));
  }
}

public sealed class CreateDeliveryTaskHandler(
  IRepository<DeliveryTask> taskRepository,
  IReadRepository<Rental> rentalRepository,
  IReadRepository<UserAccount> userRepository)
  : ICommandHandler<CreateDeliveryTaskCommand, Result<DeliveryTaskDto>>
{
  public async ValueTask<Result<DeliveryTaskDto>> Handle(CreateDeliveryTaskCommand command, CancellationToken cancellationToken)
  {
    var rental = await rentalRepository.FirstOrDefaultAsync(new RentalByIdSpec(command.RentalId), cancellationToken);
    var partner = await userRepository.FirstOrDefaultAsync(new UserAccountByIdSpec(command.DeliveryPartnerAccountId), cancellationToken);

    if (rental is null || partner is null) return Result.NotFound();
    if (partner.Role != AccountRole.DeliveryPartner) return Result.Error("The selected user is not a delivery partner.");

    var deliveryAddress = new Core.SharedKernel.DeliveryAddressSnapshot(
      rental.DeliveryAddress.Street,
      rental.DeliveryAddress.City,
      rental.DeliveryAddress.State,
      rental.DeliveryAddress.PostalCode,
      rental.DeliveryAddress.Country);

    var created = await taskRepository.AddAsync(new DeliveryTask(rental.Id, partner.Id, command.Type, deliveryAddress, command.AssignedAt), cancellationToken);
    return Result.Success(DeliveryTaskMappings.Map(created));
  }
}

public sealed class MarkDeliveryTaskInTransitHandler(IRepository<DeliveryTask> repository)
  : ICommandHandler<MarkDeliveryTaskInTransitCommand, Result<DeliveryTaskDto>>
{
  public async ValueTask<Result<DeliveryTaskDto>> Handle(MarkDeliveryTaskInTransitCommand command, CancellationToken cancellationToken)
  {
    var task = await repository.FirstOrDefaultAsync(new DeliveryTaskByIdSpec(command.DeliveryTaskId), cancellationToken);
    if (task is null) return Result.NotFound();
    try
    {
      task.MarkInTransit();
      await repository.UpdateAsync(task, cancellationToken);
      return Result.Success(DeliveryTaskMappings.Map(task));
    }
    catch (InvalidOperationException ex)
    {
      return Result.Error(ex.Message);
    }
  }
}

public sealed class CompleteDeliveryTaskHandler(
  IRepository<DeliveryTask> taskRepository,
  IRepository<Rental> rentalRepository,
  IRepository<Book> bookRepository)
  : ICommandHandler<CompleteDeliveryTaskCommand, Result<DeliveryTaskDto>>
{
  public async ValueTask<Result<DeliveryTaskDto>> Handle(CompleteDeliveryTaskCommand command, CancellationToken cancellationToken)
  {
    var task = await taskRepository.FirstOrDefaultAsync(new DeliveryTaskByIdSpec(command.DeliveryTaskId), cancellationToken);
    if (task is null) return Result.NotFound();

    var rental = await rentalRepository.FirstOrDefaultAsync(new RentalByIdSpec(task.RentalId), cancellationToken);
    if (rental is null) return Result.NotFound();

    try
    {
      task.Complete(command.CompletedAt);
      if (task.Type == DeliveryTaskType.Delivery)
      {
        rental.MarkDelivered();
        await rentalRepository.UpdateAsync(rental, cancellationToken);
      }
      else if (task.Type == DeliveryTaskType.Pickup && rental.Status != RentalStatus.Returned)
      {
        var book = await bookRepository.FirstOrDefaultAsync(new BookByIdSpec(rental.BookId), cancellationToken);
        if (book is null) return Result.NotFound();

        rental.CompleteReturn(DateOnly.FromDateTime(command.CompletedAt.UtcDateTime));
        book.ReturnCopy();
        await rentalRepository.UpdateAsync(rental, cancellationToken);
        await bookRepository.UpdateAsync(book, cancellationToken);
      }

      await taskRepository.UpdateAsync(task, cancellationToken);
      return Result.Success(DeliveryTaskMappings.Map(task));
    }
    catch (InvalidOperationException ex)
    {
      return Result.Error(ex.Message);
    }
  }
}

public sealed class FailDeliveryTaskHandler(IRepository<DeliveryTask> repository)
  : ICommandHandler<FailDeliveryTaskCommand, Result<DeliveryTaskDto>>
{
  public async ValueTask<Result<DeliveryTaskDto>> Handle(FailDeliveryTaskCommand command, CancellationToken cancellationToken)
  {
    var task = await repository.FirstOrDefaultAsync(new DeliveryTaskByIdSpec(command.DeliveryTaskId), cancellationToken);
    if (task is null) return Result.NotFound();
    try
    {
      task.Fail();
      await repository.UpdateAsync(task, cancellationToken);
      return Result.Success(DeliveryTaskMappings.Map(task));
    }
    catch (InvalidOperationException ex)
    {
      return Result.Error(ex.Message);
    }
  }
}

internal static class DeliveryTaskMappings
{
  public static DeliveryTaskDto Map(DeliveryTask task) =>
    new(
      task.Id.Value,
      task.RentalId.Value,
      task.DeliveryPartnerAccountId.Value,
      task.Type.Name,
      task.Status.Name,
      task.AssignedAt,
      task.CompletedAt,
      new AddressDto(task.DeliveryAddress.Street, task.DeliveryAddress.City, task.DeliveryAddress.State, task.DeliveryAddress.PostalCode, task.DeliveryAddress.Country));
}
