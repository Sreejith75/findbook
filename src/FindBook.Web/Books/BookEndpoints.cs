using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.LibraryInventory.CategoryAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.UseCases.Books;
using FindBook.Web.Api;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace FindBook.Web.Books;

public static class BookEndpoints
{
  public static IEndpointRouteBuilder MapBookEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/books").WithTags("Books");

    group.MapGet("/", async Task<HttpResult> (string? q, int? libraryId, int? categoryId, bool? availableOnly, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new ListBooksQuery(q, libraryId.HasValue ? LibraryId.From(libraryId.Value) : null, categoryId.HasValue ? CategoryId.From(categoryId.Value) : null, availableOnly == true), cancellationToken))
        .ToHttpResult(items => TypedResults.Ok(items.Select(MapResponse))));

    group.MapGet("/{id:int:min(1)}", async Task<HttpResult> (int id, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new GetBookByIdQuery(BookId.From(id)), cancellationToken)).ToHttpResult(item => TypedResults.Ok(MapResponse(item))));

    group.MapPost("/", async Task<HttpResult> (UpsertBookRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var parseResult = TryParseRequest(request);
      if (parseResult.ErrorResult is not null) return parseResult.ErrorResult;

      var result = await mediator.Send(new CreateBookCommand(
        LibraryId.From(request.LibraryId),
        CategoryId.From(request.CategoryId),
        parseResult.Title!.Value,
        parseResult.Author!.Value,
        parseResult.Isbn!.Value,
        parseResult.InventoryState!,
        parseResult.Description,
        parseResult.CoverImageReference), cancellationToken);

      return result.ToHttpResult(item => TypedResults.Created($"/api/books/{item.Id}", MapResponse(item)));
    });

    group.MapPut("/{id:int:min(1)}", async Task<HttpResult> (int id, UpsertBookRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var parseResult = TryParseRequest(request);
      if (parseResult.ErrorResult is not null) return parseResult.ErrorResult;

      var result = await mediator.Send(new UpdateBookCommand(
        BookId.From(id),
        LibraryId.From(request.LibraryId),
        CategoryId.From(request.CategoryId),
        parseResult.Title!.Value,
        parseResult.Author!.Value,
        parseResult.Isbn!.Value,
        parseResult.InventoryState!,
        parseResult.Description,
        parseResult.CoverImageReference), cancellationToken);

      return result.ToHttpResult(item => TypedResults.Ok(MapResponse(item)));
    });

    return app;
  }

  private static (BookTitle? Title, AuthorName? Author, Isbn? Isbn, InventoryState? InventoryState, BookDescription? Description, CoverImageReference? CoverImageReference, HttpResult? ErrorResult) TryParseRequest(UpsertBookRequest request)
  {
    var errors = new ValidationErrorBuilder();
    ApiValidation.TryCreate(() => BookTitle.From(request.Title), nameof(request.Title), errors, out var title);
    ApiValidation.TryCreate(() => AuthorName.From(request.Author), nameof(request.Author), errors, out var author);
    ApiValidation.TryCreate(() => Isbn.From(request.Isbn), nameof(request.Isbn), errors, out var isbn);
    ApiValidation.TryCreate(() => new InventoryState(request.TotalCopies, request.AvailableCopies), "inventory", errors, out var inventoryState);

    BookDescription? description = null;
    if (!string.IsNullOrWhiteSpace(request.Description))
    {
      ApiValidation.TryCreate(() => BookDescription.From(request.Description), nameof(request.Description), errors, out BookDescription parsedDescription);
      description = parsedDescription;
    }

    CoverImageReference? coverImageReference = null;
    if (!string.IsNullOrWhiteSpace(request.CoverImageReference))
    {
      ApiValidation.TryCreate(() => CoverImageReference.From(request.CoverImageReference), nameof(request.CoverImageReference), errors, out CoverImageReference parsedCoverImageReference);
      coverImageReference = parsedCoverImageReference;
    }

    return errors.HasErrors ? (null, null, null, null, null, null, ApiValidation.ValidationProblem(errors)) : (title, author, isbn, inventoryState, description, coverImageReference, null);
  }

  private static BookResponse MapResponse(BookDto book) =>
    new(book.Id, book.LibraryId, book.CategoryId, book.Title, book.Author, book.Isbn, book.Description, book.CoverImageReference, book.TotalCopies, book.AvailableCopies, book.IsAvailable, book.AverageRating, book.RatingCount, book.CoverImageUrl);
}

public sealed record UpsertBookRequest(int LibraryId, int CategoryId, string Title, string Author, string Isbn, string? Description, string? CoverImageReference, int TotalCopies, int AvailableCopies);
public sealed record BookResponse(int Id, int LibraryId, int CategoryId, string Title, string Author, string Isbn, string? Description, string? CoverImageReference, int TotalCopies, int AvailableCopies, bool IsAvailable, decimal AverageRating, int RatingCount, string CoverImageUrl);
