namespace FindBook.Core.LibraryInventory.BookAggregate;

public class BookImage : EntityBase<BookImage, BookImageId>, IAggregateRoot
{
  private BookImage() { }

  public BookImage(
    BookId bookId,
    string contentType,
    string? fileName,
    byte[] data,
    long sizeInBytes)
  {
    BookId = bookId;
    UpdateContent(contentType, fileName, data, sizeInBytes);
  }

  public BookId BookId { get; private set; }
  public string ContentType { get; private set; } = string.Empty;
  public string? FileName { get; private set; }
  public byte[] Data { get; private set; } = [];
  public long SizeInBytes { get; private set; }
  public DateTimeOffset CreatedOn { get; private set; }

  public void UpdateContent(
    string contentType,
    string? fileName,
    byte[] data,
    long sizeInBytes)
  {
    Guard.Against.NullOrWhiteSpace(contentType);
    Guard.Against.Null(data);
    Guard.Against.NegativeOrZero(sizeInBytes);

    ContentType = contentType;
    FileName = string.IsNullOrWhiteSpace(fileName) ? null : fileName;
    Data = data;
    SizeInBytes = sizeInBytes;
    CreatedOn = DateTimeOffset.UtcNow;
  }
}
