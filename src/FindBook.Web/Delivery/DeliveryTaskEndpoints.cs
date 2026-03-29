using FindBook.Core.Delivery.DeliveryTaskAggregate;
using FindBook.Core.Rental.RentalAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate;
using FindBook.UseCases.Delivery;
using FindBook.UseCases.Rentals;
using FindBook.Web.Api;
using FindBook.Web.Auth;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace FindBook.Web.Delivery;

public static class DeliveryTaskEndpoints
{
  public static IEndpointRouteBuilder MapDeliveryTaskEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/delivery-tasks").WithTags("Delivery").RequireAuthorization(FindBookPolicies.Authenticated);

    group.MapGet("/", async Task<HttpResult> (HttpContext httpContext, int? deliveryPartnerAccountId, string? status, string? type, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var currentUser = await httpContext.GetCurrentFindBookUserAsync(cancellationToken);
      if (currentUser is null)
      {
        return TypedResults.Forbid();
      }

      DeliveryTaskStatus? parsedStatus = null;
      DeliveryTaskType? parsedType = null;

      if (!string.IsNullOrWhiteSpace(status))
      {
        var errors = new ValidationErrorBuilder();
        if (!ApiValidation.TryParseSmartEnum(status, nameof(status), errors, out DeliveryTaskStatus statusValue))
        {
          return ApiValidation.ValidationProblem(errors);
        }

        parsedStatus = statusValue;
      }

      if (!string.IsNullOrWhiteSpace(type))
      {
        var errors = new ValidationErrorBuilder();
        if (!ApiValidation.TryParseSmartEnum(type, nameof(type), errors, out DeliveryTaskType typeValue))
        {
          return ApiValidation.ValidationProblem(errors);
        }

        parsedType = typeValue;
      }

      UserAccountId? effectivePartnerId = currentUser.IsAdminLike && deliveryPartnerAccountId.HasValue
        ? UserAccountId.From(deliveryPartnerAccountId.Value)
        : null;

      var result = await mediator.Send(new ListDeliveryTasksQuery(effectivePartnerId, parsedStatus, parsedType), cancellationToken);
      if (result.Status != ResultStatus.Ok)
      {
        return result.ToHttpResult(items => TypedResults.Ok(items.Select(MapResponse)));
      }

      if (currentUser.IsAdminLike || currentUser.IsDeliveryPartner)
      {
        return TypedResults.Ok(result.Value.Select(MapResponse));
      }

      var rentals = await mediator.Send(new ListRentalsQuery(currentUser.UserId, null, null), cancellationToken);
      if (rentals.Status != ResultStatus.Ok)
      {
        return TypedResults.Ok(Array.Empty<DeliveryTaskResponse>());
      }

      var allowedRentalIds = rentals.Value.Select(rental => rental.Id).ToHashSet();
      var filteredItems = result.Value.Where(item => allowedRentalIds.Contains(item.RentalId)).Select(MapResponse).ToArray();
      return TypedResults.Ok(filteredItems);
    });

    group.MapGet("/{id:int:min(1)}", async Task<HttpResult> (HttpContext httpContext, int id, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var currentUser = await httpContext.GetCurrentFindBookUserAsync(cancellationToken);
      if (currentUser is null)
      {
        return TypedResults.Forbid();
      }

      var result = await mediator.Send(new GetDeliveryTaskByIdQuery(DeliveryTaskId.From(id)), cancellationToken);
      if (result.Status != ResultStatus.Ok)
      {
        return result.ToHttpResult(item => TypedResults.Ok(MapResponse(item)));
      }

      if (currentUser.IsDeliveryPartner && result.Value.DeliveryPartnerAccountId != currentUser.UserId.Value)
      {
        return TypedResults.Forbid();
      }

      if (!currentUser.IsAdminLike && !currentUser.IsDeliveryPartner)
      {
        var rental = await mediator.Send(new GetRentalByIdQuery(RentalId.From(result.Value.RentalId)), cancellationToken);
        if (rental.Status != ResultStatus.Ok || rental.Value.UserAccountId != currentUser.UserId.Value)
        {
          return TypedResults.Forbid();
        }
      }

      return TypedResults.Ok(MapResponse(result.Value));
    });

    group.MapPost("/", async Task<HttpResult> (CreateDeliveryTaskRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var errors = new ValidationErrorBuilder();
      ApiValidation.TryParseSmartEnum(request.Type, nameof(request.Type), errors, out DeliveryTaskType parsedType);
      if (errors.HasErrors)
      {
        return ApiValidation.ValidationProblem(errors);
      }

      var result = await mediator.Send(new CreateDeliveryTaskCommand(RentalId.From(request.RentalId), UserAccountId.From(request.DeliveryPartnerAccountId), parsedType, request.AssignedAt), cancellationToken);
      return result.ToHttpResult(item => TypedResults.Created($"/api/delivery-tasks/{item.Id}", MapResponse(item)));
    }).RequireAuthorization(FindBookPolicies.Admin);

    group.MapPost("/{id:int:min(1)}/mark-in-transit", async Task<HttpResult> (HttpContext httpContext, int id, IMediator mediator, CancellationToken cancellationToken) =>
      await ExecuteScopedTaskMutationAsync(httpContext, mediator, DeliveryTaskId.From(id), cancellationToken, commandId => new MarkDeliveryTaskInTransitCommand(commandId), MapResponse));

    group.MapPost("/{id:int:min(1)}/complete", async Task<HttpResult> (HttpContext httpContext, int id, CompleteDeliveryTaskRequest request, IMediator mediator, CancellationToken cancellationToken) =>
      await ExecuteScopedTaskMutationAsync(httpContext, mediator, DeliveryTaskId.From(id), cancellationToken, commandId => new CompleteDeliveryTaskCommand(commandId, request.CompletedAt), MapResponse));

    group.MapPost("/{id:int:min(1)}/fail", async Task<HttpResult> (HttpContext httpContext, int id, IMediator mediator, CancellationToken cancellationToken) =>
      await ExecuteScopedTaskMutationAsync(httpContext, mediator, DeliveryTaskId.From(id), cancellationToken, commandId => new FailDeliveryTaskCommand(commandId), MapResponse));

    return app;
  }

  private static async Task<HttpResult> ExecuteScopedTaskMutationAsync<TCommand>(
    HttpContext httpContext,
    IMediator mediator,
    DeliveryTaskId deliveryTaskId,
    CancellationToken cancellationToken,
    Func<DeliveryTaskId, TCommand> commandFactory,
    Func<DeliveryTaskDto, DeliveryTaskResponse> mapper)
    where TCommand : Mediator.ICommand<Result<DeliveryTaskDto>>
  {
    var currentUser = await httpContext.GetCurrentFindBookUserAsync(cancellationToken);
    if (currentUser is null || (!currentUser.IsAdminLike && !currentUser.IsDeliveryPartner))
    {
      return TypedResults.Forbid();
    }

    if (currentUser.IsDeliveryPartner)
    {
      var existing = await mediator.Send(new GetDeliveryTaskByIdQuery(deliveryTaskId), cancellationToken);
      if (existing.Status != ResultStatus.Ok)
      {
        return existing.ToHttpResult(item => TypedResults.Ok(mapper(item)));
      }

      if (existing.Value.DeliveryPartnerAccountId != currentUser.UserId.Value)
      {
        return TypedResults.Forbid();
      }
    }

    var result = await mediator.Send(commandFactory(deliveryTaskId), cancellationToken);
    return result.ToHttpResult(item => TypedResults.Ok(mapper(item)));
  }

  private static DeliveryTaskResponse MapResponse(DeliveryTaskDto task) =>
    new(
      task.Id,
      task.RentalId,
      task.DeliveryPartnerAccountId,
      task.Type,
      task.Status,
      task.AssignedAt,
      task.CompletedAt,
      new AddressView(task.DeliveryAddress.Street, task.DeliveryAddress.City, task.DeliveryAddress.State, task.DeliveryAddress.PostalCode, task.DeliveryAddress.Country));
}

public sealed record CreateDeliveryTaskRequest(int RentalId, int DeliveryPartnerAccountId, string Type, DateTimeOffset AssignedAt);
public sealed record CompleteDeliveryTaskRequest(DateTimeOffset CompletedAt);
public sealed record DeliveryTaskResponse(int Id, int RentalId, int DeliveryPartnerAccountId, string Type, string Status, DateTimeOffset AssignedAt, DateTimeOffset? CompletedAt, AddressView DeliveryAddress);
