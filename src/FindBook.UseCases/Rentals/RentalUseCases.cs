using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.LibraryInventory.BookAggregate.Specifications;
using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate.Specifications;
using FindBook.Core.Rental.RentalAggregate;
using FindBook.Core.Rental.RentalAggregate.Specifications;
using FindBook.Core.SharedKernel;
using FindBook.Core.UserManagement.UserAccountAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate.Specifications;
using FindBook.UseCases.Libraries;

namespace FindBook.UseCases.Rentals;

public sealed record RentalDto(
  int Id,
  int UserAccountId,
  int BookId,
  int LibraryId,
  string Status,
  DateOnly RentedOn,
  DateOnly DueOn,
  DateOnly? ReturnRequestedOn,
  DateOnly? ReturnedOn,
  AddressDto DeliveryAddress);

public sealed record ListRentalsQuery(UserAccountId? UserAccountId, BookId? BookId, RentalStatus? Status) : IQuery<Result<IReadOnlyCollection<RentalDto>>>;
public sealed record GetRentalByIdQuery(RentalId RentalId) : IQuery<Result<RentalDto>>;
public sealed record CreateRentalCommand(UserAccountId UserAccountId, BookId BookId, LibraryId LibraryId, DeliveryAddressSnapshot DeliveryAddress, DateOnly RentedOn) : ICommand<Result<RentalDto>>;
public sealed record MarkRentalDeliveredCommand(RentalId RentalId) : ICommand<Result<RentalDto>>;
public sealed record RequestRentalReturnCommand(RentalId RentalId, DateOnly RequestedOn) : ICommand<Result<RentalDto>>;
public sealed record CompleteRentalReturnCommand(RentalId RentalId, DateOnly ReturnedOn) : ICommand<Result<RentalDto>>;
public sealed record MarkRentalOverdueCommand(RentalId RentalId, DateOnly OnDate) : ICommand<Result<RentalDto>>;

public sealed class ListRentalsHandler(IReadRepository<Rental> repository)
  : IQueryHandler<ListRentalsQuery, Result<IReadOnlyCollection<RentalDto>>>
{
  public async ValueTask<Result<IReadOnlyCollection<RentalDto>>> Handle(ListRentalsQuery query, CancellationToken cancellationToken) =>
    Result.Success<IReadOnlyCollection<RentalDto>>((await repository.ListAsync(new ListRentalsSpec(query.UserAccountId, query.BookId, query.Status), cancellationToken)).Select(RentalMappings.Map).ToArray());
}

public sealed class GetRentalByIdHandler(IReadRepository<Rental> repository)
  : IQueryHandler<GetRentalByIdQuery, Result<RentalDto>>
{
  public async ValueTask<Result<RentalDto>> Handle(GetRentalByIdQuery query, CancellationToken cancellationToken)
  {
    var rental = await repository.FirstOrDefaultAsync(new RentalByIdSpec(query.RentalId), cancellationToken);
    return rental is null ? Result.NotFound() : Result.Success(RentalMappings.Map(rental));
  }
}

public sealed class CreateRentalHandler(
  IRepository<Rental> rentalRepository,
  IRepository<Book> bookRepository,
  IReadRepository<UserAccount> userRepository,
  IReadRepository<Library> libraryRepository)
  : ICommandHandler<CreateRentalCommand, Result<RentalDto>>
{
  public async ValueTask<Result<RentalDto>> Handle(CreateRentalCommand command, CancellationToken cancellationToken)
  {
    if (await userRepository.FirstOrDefaultAsync(new UserAccountByIdSpec(command.UserAccountId), cancellationToken) is null ||
        await libraryRepository.FirstOrDefaultAsync(new LibraryByIdSpec(command.LibraryId), cancellationToken) is null)
    {
      return Result.NotFound();
    }

    var book = await bookRepository.FirstOrDefaultAsync(new BookByIdSpec(command.BookId), cancellationToken);
    if (book is null || book.LibraryId != command.LibraryId)
    {
      return Result.NotFound();
    }

    try
    {
      book.ReserveCopy();
      await bookRepository.UpdateAsync(book, cancellationToken);

      var rental = await rentalRepository.AddAsync(Rental.Create(command.UserAccountId, command.BookId, command.LibraryId, command.DeliveryAddress, command.RentedOn), cancellationToken);
      return Result.Success(RentalMappings.Map(rental));
    }
    catch (InvalidOperationException ex)
    {
      return Result.Error(ex.Message);
    }
  }
}

public sealed class MarkRentalDeliveredHandler(IRepository<Rental> repository)
  : ICommandHandler<MarkRentalDeliveredCommand, Result<RentalDto>>
{
  public async ValueTask<Result<RentalDto>> Handle(MarkRentalDeliveredCommand command, CancellationToken cancellationToken)
  {
    var rental = await repository.FirstOrDefaultAsync(new RentalByIdSpec(command.RentalId), cancellationToken);
    if (rental is null) return Result.NotFound();
    try
    {
      rental.MarkDelivered();
      await repository.UpdateAsync(rental, cancellationToken);
      return Result.Success(RentalMappings.Map(rental));
    }
    catch (InvalidOperationException ex)
    {
      return Result.Error(ex.Message);
    }
  }
}

public sealed class RequestRentalReturnHandler(IRepository<Rental> repository)
  : ICommandHandler<RequestRentalReturnCommand, Result<RentalDto>>
{
  public async ValueTask<Result<RentalDto>> Handle(RequestRentalReturnCommand command, CancellationToken cancellationToken)
  {
    var rental = await repository.FirstOrDefaultAsync(new RentalByIdSpec(command.RentalId), cancellationToken);
    if (rental is null) return Result.NotFound();
    try
    {
      rental.RequestReturn(command.RequestedOn);
      await repository.UpdateAsync(rental, cancellationToken);
      return Result.Success(RentalMappings.Map(rental));
    }
    catch (InvalidOperationException ex)
    {
      return Result.Error(ex.Message);
    }
  }
}

public sealed class CompleteRentalReturnHandler(IRepository<Rental> rentalRepository, IRepository<Book> bookRepository)
  : ICommandHandler<CompleteRentalReturnCommand, Result<RentalDto>>
{
  public async ValueTask<Result<RentalDto>> Handle(CompleteRentalReturnCommand command, CancellationToken cancellationToken)
  {
    var rental = await rentalRepository.FirstOrDefaultAsync(new RentalByIdSpec(command.RentalId), cancellationToken);
    if (rental is null) return Result.NotFound();

    var book = await bookRepository.FirstOrDefaultAsync(new BookByIdSpec(rental.BookId), cancellationToken);
    if (book is null) return Result.NotFound();

    try
    {
      rental.CompleteReturn(command.ReturnedOn);
      book.ReturnCopy();
      await rentalRepository.UpdateAsync(rental, cancellationToken);
      await bookRepository.UpdateAsync(book, cancellationToken);
      return Result.Success(RentalMappings.Map(rental));
    }
    catch (InvalidOperationException ex)
    {
      return Result.Error(ex.Message);
    }
  }
}

public sealed class MarkRentalOverdueHandler(IRepository<Rental> repository)
  : ICommandHandler<MarkRentalOverdueCommand, Result<RentalDto>>
{
  public async ValueTask<Result<RentalDto>> Handle(MarkRentalOverdueCommand command, CancellationToken cancellationToken)
  {
    var rental = await repository.FirstOrDefaultAsync(new RentalByIdSpec(command.RentalId), cancellationToken);
    if (rental is null) return Result.NotFound();

    rental.MarkOverdueIfNeeded(command.OnDate);
    await repository.UpdateAsync(rental, cancellationToken);
    return Result.Success(RentalMappings.Map(rental));
  }
}

internal static class RentalMappings
{
  public static RentalDto Map(Rental rental) =>
    new(
      rental.Id.Value,
      rental.UserAccountId.Value,
      rental.BookId.Value,
      rental.LibraryId.Value,
      rental.Status.Name,
      rental.RentalPeriod.RentedOn,
      rental.RentalPeriod.DueOn,
      rental.ReturnRequestedOn,
      rental.ReturnedOn,
      new AddressDto(rental.DeliveryAddress.Street, rental.DeliveryAddress.City, rental.DeliveryAddress.State, rental.DeliveryAddress.PostalCode, rental.DeliveryAddress.Country));
}
