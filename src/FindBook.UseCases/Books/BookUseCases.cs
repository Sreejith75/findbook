using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.LibraryInventory.BookAggregate.Specifications;
using FindBook.Core.LibraryInventory.CategoryAggregate;
using FindBook.Core.LibraryInventory.CategoryAggregate.Specifications;
using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate.Specifications;

namespace FindBook.UseCases.Books;

public sealed record BookDto(
  int Id,
  int LibraryId,
  int CategoryId,
  string Title,
  string Author,
  string Isbn,
  string? Description,
  string? CoverImageReference,
  int TotalCopies,
  int AvailableCopies,
  bool IsAvailable,
  decimal AverageRating,
  int RatingCount,
  string CoverImageUrl);

public sealed record BookImageDto(int Id, int BookId, string ContentType, string? FileName, byte[] Data, long SizeInBytes, DateTimeOffset CreatedOn);

public sealed record ListBooksQuery(string? Q, LibraryId? LibraryId, CategoryId? CategoryId, bool AvailableOnly) : IQuery<Result<IReadOnlyCollection<BookDto>>>;
public sealed record GetBookByIdQuery(BookId BookId) : IQuery<Result<BookDto>>;
public sealed record CreateBookCommand(LibraryId LibraryId, CategoryId CategoryId, BookTitle Title, AuthorName Author, Isbn Isbn, InventoryState InventoryState, BookDescription? Description, CoverImageReference? CoverImageReference) : ICommand<Result<BookDto>>;
public sealed record UpdateBookCommand(BookId BookId, LibraryId LibraryId, CategoryId CategoryId, BookTitle Title, AuthorName Author, Isbn Isbn, InventoryState InventoryState, BookDescription? Description, CoverImageReference? CoverImageReference) : ICommand<Result<BookDto>>;
public sealed record UploadBookImageCommand(BookId BookId, string ContentType, string? FileName, byte[] Data, long SizeInBytes) : ICommand<Result<BookImageDto>>;
public sealed record GetBookImageQuery(BookId BookId) : IQuery<Result<BookImageDto>>;

public sealed class ListBooksHandler(IReadRepository<Book> repository)
  : IQueryHandler<ListBooksQuery, Result<IReadOnlyCollection<BookDto>>>
{
  public async ValueTask<Result<IReadOnlyCollection<BookDto>>> Handle(ListBooksQuery query, CancellationToken cancellationToken)
  {
    var books = await repository.ListAsync(new ListBooksSpec(query.Q, query.LibraryId, query.CategoryId, query.AvailableOnly), cancellationToken);
    return Result.Success<IReadOnlyCollection<BookDto>>(books.Select(BookMappings.Map).ToArray());
  }
}

public sealed class GetBookByIdHandler(IReadRepository<Book> repository)
  : IQueryHandler<GetBookByIdQuery, Result<BookDto>>
{
  public async ValueTask<Result<BookDto>> Handle(GetBookByIdQuery query, CancellationToken cancellationToken)
  {
    var book = await repository.FirstOrDefaultAsync(new BookByIdSpec(query.BookId), cancellationToken);
    return book is null ? Result.NotFound() : Result.Success(BookMappings.Map(book));
  }
}

public sealed class CreateBookHandler(
  IRepository<Book> bookRepository,
  IReadRepository<Library> libraryRepository,
  IReadRepository<Category> categoryRepository)
  : ICommandHandler<CreateBookCommand, Result<BookDto>>
{
  public async ValueTask<Result<BookDto>> Handle(CreateBookCommand command, CancellationToken cancellationToken)
  {
    if (await libraryRepository.FirstOrDefaultAsync(new LibraryByIdSpec(command.LibraryId), cancellationToken) is null ||
        await categoryRepository.FirstOrDefaultAsync(new CategoryByIdSpec(command.CategoryId), cancellationToken) is null)
    {
      return Result.NotFound();
    }

    var duplicate = await bookRepository.FirstOrDefaultAsync(new BookByIsbnSpec(command.Isbn), cancellationToken);
    if (duplicate is not null) return Result.Conflict("A book with the same ISBN already exists.");

    var created = await bookRepository.AddAsync(new Book(command.LibraryId, command.CategoryId, command.Title, command.Author, command.Isbn, command.InventoryState, command.Description, command.CoverImageReference), cancellationToken);
    return Result.Success(BookMappings.Map(created));
  }
}

public sealed class UpdateBookHandler(
  IRepository<Book> bookRepository,
  IReadRepository<Library> libraryRepository,
  IReadRepository<Category> categoryRepository)
  : ICommandHandler<UpdateBookCommand, Result<BookDto>>
{
  public async ValueTask<Result<BookDto>> Handle(UpdateBookCommand command, CancellationToken cancellationToken)
  {
    var book = await bookRepository.FirstOrDefaultAsync(new BookByIdSpec(command.BookId), cancellationToken);
    if (book is null) return Result.NotFound();

    if (await libraryRepository.FirstOrDefaultAsync(new LibraryByIdSpec(command.LibraryId), cancellationToken) is null ||
        await categoryRepository.FirstOrDefaultAsync(new CategoryByIdSpec(command.CategoryId), cancellationToken) is null)
    {
      return Result.NotFound();
    }

    var duplicate = await bookRepository.FirstOrDefaultAsync(new BookByIsbnSpec(command.Isbn), cancellationToken);
    if (duplicate is not null && duplicate.Id != book.Id) return Result.Conflict("A book with the same ISBN already exists.");

    book.UpdateDetails(command.CategoryId, command.Title, command.Author, command.Isbn, command.InventoryState, command.Description, command.CoverImageReference);
    await bookRepository.UpdateAsync(book, cancellationToken);
    return Result.Success(BookMappings.Map(book));
  }
}

public sealed class UploadBookImageHandler(
  IReadRepository<Book> bookRepository,
  IRepository<BookImage> imageRepository)
  : ICommandHandler<UploadBookImageCommand, Result<BookImageDto>>
{
  public async ValueTask<Result<BookImageDto>> Handle(UploadBookImageCommand command, CancellationToken cancellationToken)
  {
    if (await bookRepository.FirstOrDefaultAsync(new BookByIdSpec(command.BookId), cancellationToken) is null)
    {
      return Result.NotFound();
    }

    var existing = await imageRepository.FirstOrDefaultAsync(new BookImageByBookIdSpec(command.BookId), cancellationToken);
    if (existing is null)
    {
      existing = new BookImage(command.BookId, command.ContentType, command.FileName, command.Data, command.SizeInBytes);
      existing = await imageRepository.AddAsync(existing, cancellationToken);
    }
    else
    {
      existing.UpdateContent(command.ContentType, command.FileName, command.Data, command.SizeInBytes);
      await imageRepository.UpdateAsync(existing, cancellationToken);
    }

    return Result.Success(BookMappings.Map(existing));
  }
}

public sealed class GetBookImageHandler(IReadRepository<BookImage> repository)
  : IQueryHandler<GetBookImageQuery, Result<BookImageDto>>
{
  public async ValueTask<Result<BookImageDto>> Handle(GetBookImageQuery query, CancellationToken cancellationToken)
  {
    var image = await repository.FirstOrDefaultAsync(new BookImageByBookIdSpec(query.BookId), cancellationToken);
    return image is null ? Result.NotFound() : Result.Success(BookMappings.Map(image));
  }
}

internal static class BookMappings
{
  public static BookDto Map(Book book) =>
    new(
      book.Id.Value,
      book.LibraryId.Value,
      book.CategoryId.Value,
      book.Title.Value,
      book.Author.Value,
      book.Isbn.Value,
      book.Description?.Value,
      book.CoverImageReference?.Value,
      book.InventoryState.TotalCopies,
      book.InventoryState.AvailableCopies,
      book.IsAvailable,
      book.RatingSummary.AverageRating,
      book.RatingSummary.RatingCount,
      $"/api/books/{book.Id.Value}/cover");

  public static BookImageDto Map(BookImage image) =>
    new(image.Id.Value, image.BookId.Value, image.ContentType, image.FileName, image.Data, image.SizeInBytes, image.CreatedOn);
}
