using FindBook.Core.Feedback.BookReviewAggregate;
using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.Infrastructure.Data.Config;

public class BookReviewConfiguration : IEntityTypeConfiguration<BookReview>
{
  public void Configure(EntityTypeBuilder<BookReview> builder)
  {
    builder.ToTable("BookReviews");

    // Primary Key
    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .HasColumnName("Id")
      .ValueGeneratedNever()
      .HasConversion(id => id.Value, value => BookReviewId.From(value))
      .IsRequired();

    // FK: BookId
    builder.Property(e => e.BookId)
      .HasColumnName("BookId")
      .HasConversion(id => id.Value, value => BookId.From(value))
      .IsRequired();

    // FK: ReviewerAccountId
    builder.Property(e => e.ReviewerAccountId)
      .HasColumnName("ReviewerAccountId")
      .HasConversion(id => id.Value, value => UserAccountId.From(value))
      .IsRequired();

    // StarRating (Vogen int value object, range 1-5)
    builder.Property(e => e.Rating)
      .HasColumnName("Rating")
      .HasConversion(v => v.Value, v => StarRating.From(v))
      .IsRequired();

    // ReviewTitle
    builder.Property(e => e.Title)
      .HasColumnName("Title")
      .HasMaxLength(ReviewTitle.MaxLength)
      .HasConversion(v => v.Value, v => ReviewTitle.From(v))
      .IsRequired();

    // ReviewContent
    builder.Property(e => e.Content)
      .HasColumnName("Content")
      .HasMaxLength(ReviewContent.MaxLength)
      .HasConversion(v => v.Value, v => ReviewContent.From(v))
      .IsRequired();

    // Timestamps
    builder.Property(e => e.CreatedOn)
      .HasColumnName("CreatedOn")
      .IsRequired();

    builder.Property(e => e.UpdatedOn)
      .HasColumnName("UpdatedOn")
      .IsRequired(false);

    // Indexes
    builder.HasIndex(e => e.BookId);
    builder.HasIndex(e => e.ReviewerAccountId);

    // One review per user per book
    builder.HasIndex(e => new { e.BookId, e.ReviewerAccountId }).IsUnique();
  }
}
