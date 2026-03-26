using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Infrastructure.Data;
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
    var group = app.MapGroup("/books")
      .WithTags("Books");

    group.MapPost("/{bookId:int:min(1)}/cover",
      async Task<Results<Ok<BookImageUploadResponse>, NotFound, ValidationProblem>> (
        int bookId,
        IFormFile file,
        AppDbContext dbContext,
        CancellationToken cancellationToken) =>
      {
        var validationErrors = ValidateFile(file);
        if (validationErrors.Count > 0)
        {
          return TypedResults.ValidationProblem(validationErrors);
        }

        var parsedBookId = BookId.From(bookId);
        var bookExists = await dbContext.Books.AnyAsync(b => b.Id == parsedBookId, cancellationToken);
        if (!bookExists)
        {
          return TypedResults.NotFound();
        }

        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);

        var imageBytes = memoryStream.ToArray();
        var existingImage = await dbContext.BookImages
          .SingleOrDefaultAsync(i => i.BookId == parsedBookId, cancellationToken);

        if (existingImage is null)
        {
          existingImage = new BookImage(
            parsedBookId,
            file.ContentType,
            file.FileName,
            imageBytes,
            file.Length);

          dbContext.BookImages.Add(existingImage);
        }
        else
        {
          existingImage.UpdateContent(file.ContentType, file.FileName, imageBytes, file.Length);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new BookImageUploadResponse(
          existingImage.Id.Value,
          bookId,
          existingImage.ContentType,
          existingImage.FileName,
          existingImage.SizeInBytes,
          existingImage.CreatedOn,
          $"/books/{bookId}/cover"));
      })
      .Accepts<IFormFile>("multipart/form-data")
      .Produces<BookImageUploadResponse>()
      .ProducesValidationProblem()
      .Produces(StatusCodes.Status404NotFound)
      .WithSummary("Upload or replace a book cover image");

    group.MapGet("/{bookId:int:min(1)}/cover",
      async Task<Results<FileContentHttpResult, NotFound>> (
        int bookId,
        AppDbContext dbContext,
        CancellationToken cancellationToken) =>
      {
        var parsedBookId = BookId.From(bookId);

        var image = await dbContext.BookImages
          .AsNoTracking()
          .SingleOrDefaultAsync(i => i.BookId == parsedBookId, cancellationToken);

        if (image is null)
        {
          return TypedResults.NotFound();
        }

        return TypedResults.File(
          image.Data,
          image.ContentType,
          fileDownloadName: image.FileName,
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
