using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.Rental.RentalAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.Infrastructure.Data.Config;

public class RentalConfiguration : IEntityTypeConfiguration<Rental>
{
  public void Configure(EntityTypeBuilder<Rental> builder)
  {
    builder.ToTable("Rentals");

    // Primary Key
    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .HasColumnName("Id")
      .ValueGeneratedNever()
      .HasConversion(id => id.Value, value => RentalId.From(value))
      .IsRequired();

    // FK: UserAccountId
    builder.Property(e => e.UserAccountId)
      .HasColumnName("UserAccountId")
      .HasConversion(id => id.Value, value => UserAccountId.From(value))
      .IsRequired();

    // FK: BookId
    builder.Property(e => e.BookId)
      .HasColumnName("BookId")
      .HasConversion(id => id.Value, value => BookId.From(value))
      .IsRequired();

    // FK: LibraryId
    builder.Property(e => e.LibraryId)
      .HasColumnName("LibraryId")
      .HasConversion(id => id.Value, value => LibraryId.From(value))
      .IsRequired();

    // RentalStatus (SmartEnum → int)
    builder.Property(e => e.Status)
      .HasColumnName("Status")
      .HasConversion(v => v.Value, v => RentalStatus.FromValue(v))
      .IsRequired();

    // DateOnly nullable fields
    builder.Property(e => e.ReturnRequestedOn)
      .HasColumnName("ReturnRequestedOn")
      .HasColumnType("date")
      .IsRequired(false);

    builder.Property(e => e.ReturnedOn)
      .HasColumnName("ReturnedOn")
      .HasColumnType("date")
      .IsRequired(false);

    // Owned: RentalPeriod (RentedOn, DueOn)
    builder.OwnsOne(e => e.RentalPeriod, rp =>
    {
      rp.Property(p => p.RentedOn)
        .HasColumnName("RentedOn")
        .HasColumnType("date")
        .IsRequired();
      rp.Property(p => p.DueOn)
        .HasColumnName("DueOn")
        .HasColumnType("date")
        .IsRequired();
    });

    // Owned: DeliveryAddressSnapshot (5 string columns)
    builder.OwnsOne(e => e.DeliveryAddress, addr =>
    {
      addr.Property(p => p.Street).HasColumnName("DeliveryStreet").HasMaxLength(250).IsRequired();
      addr.Property(p => p.City).HasColumnName("DeliveryCity").HasMaxLength(100).IsRequired();
      addr.Property(p => p.State).HasColumnName("DeliveryState").HasMaxLength(100).IsRequired();
      addr.Property(p => p.PostalCode).HasColumnName("DeliveryPostalCode").HasMaxLength(20).IsRequired();
      addr.Property(p => p.Country).HasColumnName("DeliveryCountry").HasMaxLength(100).IsRequired();
    });

    // Indexes
    builder.HasIndex(e => e.UserAccountId);
    builder.HasIndex(e => e.BookId);
    builder.HasIndex(e => e.LibraryId);
  }
}
