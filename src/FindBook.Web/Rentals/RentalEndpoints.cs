using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.Rental.RentalAggregate;
using FindBook.Core.SharedKernel;
using FindBook.Core.UserManagement.UserAccountAggregate;
using FindBook.UseCases.Rentals;
using FindBook.Web.Api;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace FindBook.Web.Rentals;

public static class RentalEndpoints
{
  public static IEndpointRouteBuilder MapRentalEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/rentals").WithTags("Rentals").RequireAuthorization();

    group.MapGet("/", async Task<HttpResult> (int? userAccountId, int? bookId, string? status, IMediator mediator, CancellationToken cancellationToken) =>
    {
      RentalStatus? parsedStatus = null;
      if (!string.IsNullOrWhiteSpace(status))
      {
        var errors = new ValidationErrorBuilder();
        if (!ApiValidation.TryParseSmartEnum(status, nameof(status), errors, out RentalStatus value))
        {
          return ApiValidation.ValidationProblem(errors);
        }
        parsedStatus = value;
      }

      var result = await mediator.Send(new ListRentalsQuery(userAccountId.HasValue ? UserAccountId.From(userAccountId.Value) : null, bookId.HasValue ? BookId.From(bookId.Value) : null, parsedStatus), cancellationToken);
      return result.ToHttpResult(items => TypedResults.Ok(items.Select(MapResponse)));
    });

    group.MapGet("/{id:int:min(1)}", async Task<HttpResult> (int id, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new GetRentalByIdQuery(RentalId.From(id)), cancellationToken)).ToHttpResult(item => TypedResults.Ok(MapResponse(item))));

    group.MapPost("/", async Task<HttpResult> (CreateRentalRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var errors = new ValidationErrorBuilder();
      ApiValidation.TryCreate(() => new DeliveryAddressSnapshot(request.DeliveryAddress.Street, request.DeliveryAddress.City, request.DeliveryAddress.State, request.DeliveryAddress.PostalCode, request.DeliveryAddress.Country), nameof(request.DeliveryAddress), errors, out var address);
      if (errors.HasErrors) return ApiValidation.ValidationProblem(errors);

      var result = await mediator.Send(new CreateRentalCommand(UserAccountId.From(request.UserAccountId), BookId.From(request.BookId), LibraryId.From(request.LibraryId), address, request.RentedOn), cancellationToken);
      return result.ToHttpResult(item => TypedResults.Created($"/api/rentals/{item.Id}", MapResponse(item)));
    });

    group.MapPost("/{id:int:min(1)}/mark-delivered", async Task<HttpResult> (int id, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new MarkRentalDeliveredCommand(RentalId.From(id)), cancellationToken)).ToHttpResult(item => TypedResults.Ok(MapResponse(item))));

    group.MapPost("/{id:int:min(1)}/request-return", async Task<HttpResult> (int id, RequestReturnRequest request, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new RequestRentalReturnCommand(RentalId.From(id), request.RequestedOn), cancellationToken)).ToHttpResult(item => TypedResults.Ok(MapResponse(item))));

    group.MapPost("/{id:int:min(1)}/complete-return", async Task<HttpResult> (int id, CompleteReturnRequest request, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new CompleteRentalReturnCommand(RentalId.From(id), request.ReturnedOn), cancellationToken)).ToHttpResult(item => TypedResults.Ok(MapResponse(item))));

    group.MapPost("/{id:int:min(1)}/mark-overdue", async Task<HttpResult> (int id, MarkOverdueRequest request, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new MarkRentalOverdueCommand(RentalId.From(id), request.OnDate), cancellationToken)).ToHttpResult(item => TypedResults.Ok(MapResponse(item))));

    return app;
  }

  private static RentalResponse MapResponse(RentalDto rental) =>
    new(rental.Id, rental.UserAccountId, rental.BookId, rental.LibraryId, rental.Status, rental.RentedOn, rental.DueOn, rental.ReturnRequestedOn, rental.ReturnedOn, new AddressView(rental.DeliveryAddress.Street, rental.DeliveryAddress.City, rental.DeliveryAddress.State, rental.DeliveryAddress.PostalCode, rental.DeliveryAddress.Country));
}

public sealed record CreateRentalRequest(int UserAccountId, int BookId, int LibraryId, DateOnly RentedOn, AddressPayload DeliveryAddress);
public sealed record RequestReturnRequest(DateOnly RequestedOn);
public sealed record CompleteReturnRequest(DateOnly ReturnedOn);
public sealed record MarkOverdueRequest(DateOnly OnDate);
public sealed record RentalResponse(int Id, int UserAccountId, int BookId, int LibraryId, string Status, DateOnly RentedOn, DateOnly DueOn, DateOnly? ReturnRequestedOn, DateOnly? ReturnedOn, AddressView DeliveryAddress);
