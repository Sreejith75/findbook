using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.SharedKernel;
using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.Infrastructure.Data.Config;

public class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
  public void Configure(EntityTypeBuilder<UserAccount> builder)
  {
    builder.ToTable("UserAccounts");

    // Primary Key
    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .HasColumnName("Id")
      .ValueGeneratedNever()
      .HasConversion(id => id.Value, value => UserAccountId.From(value))
      .IsRequired();

    // FullName (Vogen string value object)
    builder.Property(e => e.FullName)
      .HasColumnName("FullName")
      .HasMaxLength(PersonName.MaxLength)
      .HasConversion(v => v.Value, v => PersonName.From(v))
      .IsRequired();

    // Email (Vogen string value object)
    builder.Property(e => e.Email)
      .HasColumnName("Email")
      .HasMaxLength(256)
      .HasConversion(v => v.Value, v => EmailAddress.From(v))
      .IsRequired();

    builder.HasIndex(e => e.Email).IsUnique();

    // PasswordHash (Vogen string value object)
    builder.Property(e => e.PasswordHash)
      .HasColumnName("PasswordHash")
      .HasMaxLength(512)
      .HasConversion(v => v.Value, v => PasswordHash.From(v))
      .IsRequired();

    // PhoneNumber (Vogen string value object — nullable, stored as single varchar column)
    builder.Property(e => e.PhoneNumber)
      .HasColumnName("PhoneNumber")
      .HasMaxLength(32)
      .HasConversion(
        v => v.HasValue ? v.Value.Value : null,
        v => v != null ? PhoneNumber.From(v) : (PhoneNumber?)null)
      .IsRequired(false);

    // AccountRole (SmartEnum → int)
    builder.Property(e => e.Role)
      .HasColumnName("Role")
      .HasConversion(v => v.Value, v => AccountRole.FromValue(v))
      .IsRequired();

    // ManagedLibraryId (nullable Vogen int FK)
    builder.Property(e => e.ManagedLibraryId)
      .HasColumnName("ManagedLibraryId")
      .HasConversion(
        v => v.HasValue ? (int?)v.Value.Value : null,
        v => v.HasValue ? LibraryId.From(v.Value) : (LibraryId?)null)
      .IsRequired(false);

    // Owned collection: SavedAddresses (own table)
    builder.OwnsMany(e => e.SavedAddresses, sa =>
    {
      sa.ToTable("SavedAddresses");
      sa.WithOwner().HasForeignKey("UserAccountId");
      sa.HasKey("UserAccountId", "Id");

      sa.Property(a => a.Id)
        .HasColumnName("Id")
        .ValueGeneratedOnAdd()
        .HasConversion(id => id.Value, value => SavedAddressId.From(value))
        .IsRequired();

      sa.Property(a => a.IsDefault)
        .HasColumnName("IsDefault")
        .IsRequired();

      // PostalAddress (ref-type ValueObject — 5 owned columns inline)
      sa.OwnsOne(a => a.Address, addr =>
      {
        addr.Property(p => p.Street).HasColumnName("Street").HasMaxLength(250).IsRequired();
        addr.Property(p => p.City).HasColumnName("City").HasMaxLength(100).IsRequired();
        addr.Property(p => p.State).HasColumnName("State").HasMaxLength(100).IsRequired();
        addr.Property(p => p.PostalCode).HasColumnName("PostalCode").HasMaxLength(20).IsRequired();
        addr.Property(p => p.Country).HasColumnName("Country").HasMaxLength(100).IsRequired();
      });
    });
  }
}
