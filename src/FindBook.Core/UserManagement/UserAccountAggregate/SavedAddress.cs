using FindBook.Core.SharedKernel;

namespace FindBook.Core.UserManagement.UserAccountAggregate;

public class SavedAddress : EntityBase<SavedAddress, SavedAddressId>
{
  private SavedAddress() { }

  internal SavedAddress(SavedAddressId id, PostalAddress address, bool isDefault)
  {
    Id = id;
    Address = address;
    IsDefault = isDefault;
  }

  public PostalAddress Address { get; private set; } = null!;
  public bool IsDefault { get; private set; }

  internal void UpdateAddress(PostalAddress address) => Address = address;

  internal void MarkDefault() => IsDefault = true;

  internal void UnmarkDefault() => IsDefault = false;
}
