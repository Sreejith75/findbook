using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.LibraryInventory.CategoryAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate;

namespace FindBook.Infrastructure.Data.Config;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
  public void Configure(EntityTypeBuilder<Book> builder)
  {
    builder.ToTable("Books");

    // Primary Key
    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .HasColumnName("Id")
      .ValueGeneratedNever()
      .HasConversion(id => id.Value, value => BookId.From(value))
      .IsRequired();

    // FK: LibraryId
    builder.Property(e => e.LibraryId)
      .HasColumnName("LibraryId")
      .HasConversion(id => id.Value, value => LibraryId.From(value))
      .IsRequired();

    // FK: CategoryId
    builder.Property(e => e.CategoryId)
      .HasColumnName("CategoryId")
      .HasConversion(id => id.Value, value => CategoryId.From(value))
      .IsRequired();

    // BookTitle
    builder.Property(e => e.Title)
      .HasColumnName("Title")
      .HasMaxLength(BookTitle.MaxLength)
      .HasConversion(v => v.Value, v => BookTitle.From(v))
      .IsRequired();

    // AuthorName
    builder.Property(e => e.Author)
      .HasColumnName("Author")
      .HasMaxLength(AuthorName.MaxLength)
      .HasConversion(v => v.Value, v => AuthorName.From(v))
      .IsRequired();

    // ISBN (max 17 chars per regex pattern)
    builder.Property(e => e.Isbn)
      .HasColumnName("Isbn")
      .HasMaxLength(20)
      .HasConversion(v => v.Value, v => Isbn.From(v))
      .IsRequired();

    builder.HasIndex(e => e.Isbn).IsUnique();

    // BookDescription (nullable)
    builder.Property(e => e.Description)
      .HasColumnName("Description")
      .HasMaxLength(BookDescription.MaxLength)
      .HasConversion(
        v => v.HasValue ? v.Value.Value : null,
        v => v != null ? BookDescription.From(v) : (BookDescription?)null)
      .IsRequired(false);

    // CoverImageReference (nullable)
    builder.Property(e => e.CoverImageReference)
      .HasColumnName("CoverImageUrl")
      .HasMaxLength(500)
      .HasConversion(
        v => v.HasValue ? v.Value.Value : null,
        v => v != null ? CoverImageReference.From(v) : (CoverImageReference?)null)
      .IsRequired(false);

    // Owned: InventoryState (TotalCopies, AvailableCopies)
    builder.OwnsOne(e => e.InventoryState, inv =>
    {
      inv.Property(p => p.TotalCopies)
        .HasColumnName("TotalCopies")
        .IsRequired();
      inv.Property(p => p.AvailableCopies)
        .HasColumnName("AvailableCopies")
        .IsRequired();
    });

    // Owned: BookRatingSummary (AverageRating decimal, RatingCount int)
    builder.OwnsOne(e => e.RatingSummary, rating =>
    {
      rating.Property(p => p.AverageRating)
        .HasColumnName("AverageRating")
        .HasColumnType("numeric(5,2)")
        .IsRequired();
      rating.Property(p => p.RatingCount)
        .HasColumnName("RatingCount")
        .IsRequired();
    });

    // Indexes
    builder.HasIndex(e => e.LibraryId);
    builder.HasIndex(e => e.CategoryId);
  }
}
