using FindBook.Core.Delivery.DeliveryTaskAggregate;
using FindBook.Core.Rental.RentalAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.Infrastructure.Data.Config;

public class DeliveryTaskConfiguration : IEntityTypeConfiguration<DeliveryTask>
{
  public void Configure(EntityTypeBuilder<DeliveryTask> builder)
  {
    builder.ToTable("DeliveryTasks");

    // Primary Key
    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .HasColumnName("Id")
      .ValueGeneratedOnAdd()
      .HasConversion(id => id.Value, value => DeliveryTaskId.From(value))
      .IsRequired();

    // FK: RentalId
    builder.Property(e => e.RentalId)
      .HasColumnName("RentalId")
      .HasConversion(id => id.Value, value => RentalId.From(value))
      .IsRequired();

    // FK: DeliveryPartnerAccountId
    builder.Property(e => e.DeliveryPartnerAccountId)
      .HasColumnName("DeliveryPartnerAccountId")
      .HasConversion(id => id.Value, value => UserAccountId.From(value))
      .IsRequired();

    // DeliveryTaskType (SmartEnum → int)
    builder.Property(e => e.Type)
      .HasColumnName("Type")
      .HasConversion(v => v.Value, v => DeliveryTaskType.FromValue(v))
      .IsRequired();

    // DeliveryTaskStatus (SmartEnum → int)
    builder.Property(e => e.Status)
      .HasColumnName("Status")
      .HasConversion(v => v.Value, v => DeliveryTaskStatus.FromValue(v))
      .IsRequired();

    // Timestamps
    builder.Property(e => e.AssignedAt)
      .HasColumnName("AssignedAt")
      .IsRequired();

    builder.Property(e => e.CompletedAt)
      .HasColumnName("CompletedAt")
      .IsRequired(false);

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
    builder.HasIndex(e => e.RentalId);
    builder.HasIndex(e => e.DeliveryPartnerAccountId);
  }
}
