using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.SharedKernel;

namespace FindBook.Infrastructure.Data.Config;

public class LibraryConfiguration : IEntityTypeConfiguration<Library>
{
  public void Configure(EntityTypeBuilder<Library> builder)
  {
    builder.ToTable("Libraries");

    // Primary Key
    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .HasColumnName("Id")
      .ValueGeneratedOnAdd()
      .HasConversion(id => id.Value, value => LibraryId.From(value))
      .IsRequired();

    // LibraryName (Vogen string value object)
    builder.Property(e => e.Name)
      .HasColumnName("Name")
      .HasMaxLength(LibraryName.MaxLength)
      .HasConversion(v => v.Value, v => LibraryName.From(v))
      .IsRequired();

    builder.HasIndex(e => e.Name).IsUnique();

    // ContactEmail (Vogen string value object)
    builder.Property(e => e.ContactEmail)
      .HasColumnName("ContactEmail")
      .HasMaxLength(256)
      .HasConversion(v => v.Value, v => EmailAddress.From(v))
      .IsRequired();

    // ContactPhone (Vogen string value object — stored as single varchar column)
    builder.Property(e => e.ContactPhone)
      .HasColumnName("ContactPhone")
      .HasMaxLength(32)
      .HasConversion(v => v.Value, v => PhoneNumber.From(v))
      .IsRequired();

    // PostalAddress (ref-type ValueObject — 5 owned columns inline)
    builder.OwnsOne(e => e.Address, addr =>
    {
      addr.Property(p => p.Street).HasColumnName("Street").HasMaxLength(250).IsRequired();
      addr.Property(p => p.City).HasColumnName("City").HasMaxLength(100).IsRequired();
      addr.Property(p => p.State).HasColumnName("State").HasMaxLength(100).IsRequired();
      addr.Property(p => p.PostalCode).HasColumnName("PostalCode").HasMaxLength(20).IsRequired();
      addr.Property(p => p.Country).HasColumnName("Country").HasMaxLength(100).IsRequired();
    });
  }
}
