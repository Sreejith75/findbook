using FindBook.Core.LibraryInventory.BookAggregate;

namespace FindBook.Infrastructure.Data.Config;

public class BookImageConfiguration : IEntityTypeConfiguration<BookImage>
{
  public void Configure(EntityTypeBuilder<BookImage> builder)
  {
    builder.ToTable("BookImages");

    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .HasColumnName("Id")
      .ValueGeneratedOnAdd()
      .HasConversion(id => id.Value, value => BookImageId.From(value))
      .IsRequired();

    builder.Property(e => e.BookId)
      .HasColumnName("BookId")
      .HasConversion(id => id.Value, value => BookId.From(value))
      .IsRequired();

    builder.Property(e => e.ContentType)
      .HasColumnName("ContentType")
      .HasMaxLength(128)
      .IsRequired();

    builder.Property(e => e.FileName)
      .HasColumnName("FileName")
      .HasMaxLength(260)
      .IsRequired(false);

    builder.Property(e => e.Data)
      .HasColumnName("Data")
      .HasColumnType("bytea")
      .IsRequired();

    builder.Property(e => e.SizeInBytes)
      .HasColumnName("SizeInBytes")
      .IsRequired();

    builder.Property(e => e.CreatedOn)
      .HasColumnName("CreatedOn")
      .IsRequired();

    builder.HasIndex(e => e.BookId)
      .IsUnique();

    builder.HasOne<Book>()
      .WithOne()
      .HasForeignKey<BookImage>(e => e.BookId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
