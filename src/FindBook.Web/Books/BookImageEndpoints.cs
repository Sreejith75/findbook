using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.UseCases.Books;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FindBook.Web.Books;

public static class BookImageEndpoints
{
  private static readonly HashSet<string> AllowedContentTypes =
  [
    "image/jpeg",
    "image/png",
    "image/webp"
  ];

  private const long MaxImageSizeInBytes = 5 * 1024 * 1024;

  public static IEndpointRouteBuilder MapBookImageEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/books")
      .WithTags("Books");

    group.MapPost("/{bookId:int:min(1)}/cover",
      async Task<Results<Ok<BookImageUploadResponse>, NotFound, ValidationProblem>> (
        int bookId,
        IFormFile file,
        IMediator mediator,
        CancellationToken cancellationToken) =>
      {
        var validationErrors = ValidateFile(file);
        if (validationErrors.Count > 0)
        {
          return TypedResults.ValidationProblem(validationErrors);
        }

        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);

        var result = await mediator.Send(new UploadBookImageCommand(
          BookId.From(bookId),
          file.ContentType,
          file.FileName,
          memoryStream.ToArray(),
          file.Length), cancellationToken);

        if (result.Status == ResultStatus.NotFound)
        {
          return TypedResults.NotFound();
        }

        var existingImage = result.Value;
        return TypedResults.Ok(new BookImageUploadResponse(
          existingImage.Id,
          bookId,
          existingImage.ContentType,
          existingImage.FileName,
          existingImage.SizeInBytes,
          existingImage.CreatedOn,
          $"/api/books/{bookId}/cover"));
      })
      .Accepts<IFormFile>("multipart/form-data")
      .Produces<BookImageUploadResponse>()
      .ProducesValidationProblem()
      .Produces(StatusCodes.Status404NotFound)
      .WithSummary("Upload or replace a book cover image");

    group.MapGet("/{bookId:int:min(1)}/cover",
      async Task<Results<FileContentHttpResult, NotFound>> (
        int bookId,
        IMediator mediator,
        CancellationToken cancellationToken) =>
      {
        var result = await mediator.Send(new GetBookImageQuery(BookId.From(bookId)), cancellationToken);

        if (result.Status == ResultStatus.NotFound)
        {
          return TypedResults.NotFound();
        }

        return TypedResults.File(
          result.Value.Data,
          result.Value.ContentType,
          fileDownloadName: result.Value.FileName,
          enableRangeProcessing: false);
      })
      .Produces(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status404NotFound)
      .WithSummary("Get a book cover image");

    return app;
  }

  private static Dictionary<string, string[]> ValidateFile(IFormFile? file)
  {
    var errors = new Dictionary<string, string[]>();

    if (file is null)
    {
      errors["file"] = ["A file is required."];
      return errors;
    }

    if (file.Length <= 0)
    {
      errors["file"] = ["The uploaded file is empty."];
      return errors;
    }

    if (file.Length > MaxImageSizeInBytes)
    {
      errors["file"] = [$"The uploaded file exceeds the {MaxImageSizeInBytes / (1024 * 1024)} MB limit."];
    }

    if (!AllowedContentTypes.Contains(file.ContentType))
    {
      errors["contentType"] = ["Only JPEG, PNG, and WEBP images are allowed."];
    }

    return errors;
  }
}

public sealed record BookImageUploadResponse(
  int Id,
  int BookId,
  string ContentType,
  string? FileName,
  long SizeInBytes,
  DateTimeOffset CreatedOn,
  string Url);
