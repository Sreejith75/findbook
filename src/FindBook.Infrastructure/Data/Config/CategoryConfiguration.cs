using FindBook.Core.LibraryInventory.CategoryAggregate;

namespace FindBook.Infrastructure.Data.Config;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
  public void Configure(EntityTypeBuilder<Category> builder)
  {
    builder.ToTable("Categories");

    // Primary Key
    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .HasColumnName("Id")
      .ValueGeneratedNever()
      .HasConversion(id => id.Value, value => CategoryId.From(value))
      .IsRequired();

    // CategoryName (Vogen string value object)
    builder.Property(e => e.Name)
      .HasColumnName("Name")
      .HasMaxLength(CategoryName.MaxLength)
      .HasConversion(v => v.Value, v => CategoryName.From(v))
      .IsRequired();

    builder.HasIndex(e => e.Name).IsUnique();
  }
}
