using FindBook.Core.ContributorAggregate;

namespace FindBook.Infrastructure.Data.Config;

public class ContributorConfiguration : IEntityTypeConfiguration<Contributor>
{
  public void Configure(EntityTypeBuilder<Contributor> builder)
  {
    builder.ToTable("Contributors");

    // Primary Key
    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .HasColumnName("Id")
      .ValueGeneratedOnAdd()
      .HasConversion(id => id.Value, value => ContributorId.From(value))
      .IsRequired();

    // ContributorName (Vogen string value object)
    builder.Property(e => e.Name)
      .HasColumnName("Name")
      .HasMaxLength(ContributorName.MaxLength)
      .HasConversion(v => v.Value, v => ContributorName.From(v))
      .IsRequired();

    // ContributorStatus (SmartEnum → int)
    builder.Property(e => e.Status)
      .HasColumnName("Status")
      .HasConversion(v => v.Value, v => ContributorStatus.FromValue(v))
      .IsRequired();

    // PhoneNumber (nullable Vogen string owned value object)
    builder.OwnsOne(e => e.PhoneNumber, phone =>
    {
      phone.Property(p => p.Value)
        .HasColumnName("PhoneNumber")
        .HasMaxLength(30)
        .IsRequired(false);
    });
  }
}
