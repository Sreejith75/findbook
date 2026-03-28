using FindBook.Core.Delivery.DeliveryTaskAggregate;
using FindBook.Core.Rental.RentalAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate;
using FindBook.UseCases.Delivery;
using FindBook.Web.Api;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace FindBook.Web.Delivery;

public static class DeliveryTaskEndpoints
{
  public static IEndpointRouteBuilder MapDeliveryTaskEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/delivery-tasks").WithTags("Delivery").RequireAuthorization();

    group.MapGet("/", async Task<HttpResult> (int? deliveryPartnerAccountId, string? status, string? type, IMediator mediator, CancellationToken cancellationToken) =>
    {
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

      var result = await mediator.Send(new ListDeliveryTasksQuery(deliveryPartnerAccountId.HasValue ? UserAccountId.From(deliveryPartnerAccountId.Value) : null, parsedStatus, parsedType), cancellationToken);
      return result.ToHttpResult(items => TypedResults.Ok(items.Select(MapResponse)));
    });

    group.MapGet("/{id:int:min(1)}", async Task<HttpResult> (int id, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new GetDeliveryTaskByIdQuery(DeliveryTaskId.From(id)), cancellationToken)).ToHttpResult(item => TypedResults.Ok(MapResponse(item))));

    group.MapPost("/", async Task<HttpResult> (CreateDeliveryTaskRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var errors = new ValidationErrorBuilder();
      ApiValidation.TryParseSmartEnum(request.Type, nameof(request.Type), errors, out DeliveryTaskType parsedType);
      if (errors.HasErrors) return ApiValidation.ValidationProblem(errors);

      var result = await mediator.Send(new CreateDeliveryTaskCommand(RentalId.From(request.RentalId), UserAccountId.From(request.DeliveryPartnerAccountId), parsedType, request.AssignedAt), cancellationToken);
      return result.ToHttpResult(item => TypedResults.Created($"/api/delivery-tasks/{item.Id}", MapResponse(item)));
    });

    group.MapPost("/{id:int:min(1)}/mark-in-transit", async Task<HttpResult> (int id, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new MarkDeliveryTaskInTransitCommand(DeliveryTaskId.From(id)), cancellationToken)).ToHttpResult(item => TypedResults.Ok(MapResponse(item))));

    group.MapPost("/{id:int:min(1)}/complete", async Task<HttpResult> (int id, CompleteDeliveryTaskRequest request, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new CompleteDeliveryTaskCommand(DeliveryTaskId.From(id), request.CompletedAt), cancellationToken)).ToHttpResult(item => TypedResults.Ok(MapResponse(item))));

    group.MapPost("/{id:int:min(1)}/fail", async Task<HttpResult> (int id, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new FailDeliveryTaskCommand(DeliveryTaskId.From(id)), cancellationToken)).ToHttpResult(item => TypedResults.Ok(MapResponse(item))));

    return app;
  }

  private static DeliveryTaskResponse MapResponse(DeliveryTaskDto task) =>
    new(task.Id, task.RentalId, task.DeliveryPartnerAccountId, task.Type, task.Status, task.AssignedAt, task.CompletedAt, new AddressView(task.DeliveryAddress.Street, task.DeliveryAddress.City, task.DeliveryAddress.State, task.DeliveryAddress.PostalCode, task.DeliveryAddress.Country));
}

public sealed record CreateDeliveryTaskRequest(int RentalId, int DeliveryPartnerAccountId, string Type, DateTimeOffset AssignedAt);
public sealed record CompleteDeliveryTaskRequest(DateTimeOffset CompletedAt);
public sealed record DeliveryTaskResponse(int Id, int RentalId, int DeliveryPartnerAccountId, string Type, string Status, DateTimeOffset AssignedAt, DateTimeOffset? CompletedAt, AddressView DeliveryAddress);
