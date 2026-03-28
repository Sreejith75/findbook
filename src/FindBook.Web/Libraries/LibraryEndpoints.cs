using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.SharedKernel;
using FindBook.UseCases.Libraries;
using FindBook.Web.Api;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace FindBook.Web.Libraries;

public static class LibraryEndpoints
{
  public static IEndpointRouteBuilder MapLibraryEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/libraries").WithTags("Libraries").RequireAuthorization();

    group.MapGet("/", async Task<HttpResult> (IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new ListLibrariesQuery(), cancellationToken)).ToHttpResult(items => TypedResults.Ok(items.Select(MapResponse))));

    group.MapGet("/{id:int:min(1)}", async Task<HttpResult> (int id, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new GetLibraryByIdQuery(LibraryId.From(id)), cancellationToken)).ToHttpResult(item => TypedResults.Ok(MapResponse(item))));

    group.MapPost("/", async Task<HttpResult> (UpsertLibraryRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var parseResult = TryParseRequest(request);
      if (parseResult.ErrorResult is not null) return parseResult.ErrorResult;

      var result = await mediator.Send(new CreateLibraryCommand(parseResult.Name!.Value, parseResult.Address!, parseResult.Email!.Value, parseResult.PhoneNumber!.Value), cancellationToken);
      return result.ToHttpResult(item => TypedResults.Created($"/api/libraries/{item.Id}", MapResponse(item)));
    });

    group.MapPut("/{id:int:min(1)}", async Task<HttpResult> (int id, UpsertLibraryRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var parseResult = TryParseRequest(request);
      if (parseResult.ErrorResult is not null) return parseResult.ErrorResult;

      var result = await mediator.Send(new UpdateLibraryCommand(LibraryId.From(id), parseResult.Name!.Value, parseResult.Address!, parseResult.Email!.Value, parseResult.PhoneNumber!.Value), cancellationToken);
      return result.ToHttpResult(item => TypedResults.Ok(MapResponse(item)));
    });

    return app;
  }

  private static (LibraryName? Name, PostalAddress? Address, EmailAddress? Email, PhoneNumber? PhoneNumber, HttpResult? ErrorResult) TryParseRequest(UpsertLibraryRequest request)
  {
    var errors = new ValidationErrorBuilder();
    ApiValidation.TryCreate(() => LibraryName.From(request.Name), nameof(request.Name), errors, out var name);
    ApiValidation.TryCreate(() => EmailAddress.From(request.ContactEmail), nameof(request.ContactEmail), errors, out var email);
    ApiValidation.TryCreate(() => PhoneNumber.From(request.ContactPhone), nameof(request.ContactPhone), errors, out var phoneNumber);
    ApiValidation.TryCreate(() => new PostalAddress(request.Address.Street, request.Address.City, request.Address.State, request.Address.PostalCode, request.Address.Country), nameof(request.Address), errors, out var address);
    return errors.HasErrors ? (null, null, null, null, ApiValidation.ValidationProblem(errors)) : (name, address, email, phoneNumber, null);
  }

  private static LibraryResponse MapResponse(LibraryDto library) =>
    new(library.Id, library.Name, library.ContactEmail, library.ContactPhone, new AddressView(library.Address.Street, library.Address.City, library.Address.State, library.Address.PostalCode, library.Address.Country));
}

public sealed record UpsertLibraryRequest(string Name, string ContactEmail, string ContactPhone, AddressPayload Address);
public sealed record LibraryResponse(int Id, string Name, string ContactEmail, string ContactPhone, AddressView Address);
