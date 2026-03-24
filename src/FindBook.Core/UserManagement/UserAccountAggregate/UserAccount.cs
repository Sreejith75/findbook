using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.SharedKernel;

namespace FindBook.Core.UserManagement.UserAccountAggregate;

public class UserAccount : EntityBase<UserAccount, UserAccountId>, IAggregateRoot
{
  private readonly List<SavedAddress> _savedAddresses = [];

  private UserAccount() { }

  public UserAccount(
    PersonName fullName,
    EmailAddress email,
    PasswordHash passwordHash,
    AccountRole role,
    PhoneNumber? phoneNumber = null)
  {
    FullName = fullName;
    Email = email;
    PasswordHash = passwordHash;
    Role = role;
    PhoneNumber = phoneNumber;
  }

  public PersonName FullName { get; private set; }
  public EmailAddress Email { get; private set; }
  public PasswordHash PasswordHash { get; private set; }
  public PhoneNumber? PhoneNumber { get; private set; }
  public AccountRole Role { get; private set; } = AccountRole.User;
  public LibraryId? ManagedLibraryId { get; private set; }
  public IReadOnlyCollection<SavedAddress> SavedAddresses => _savedAddresses.AsReadOnly();

  public void UpdateProfile(PersonName fullName, EmailAddress email, PhoneNumber? phoneNumber)
  {
    FullName = fullName;
    Email = email;
    PhoneNumber = phoneNumber;
  }

  public void ChangeRole(AccountRole role)
  {
    Role = role;
    if (!role.CanManageLibrary)
    {
      ManagedLibraryId = null;
    }
  }

  public void AssignManagedLibrary(LibraryId libraryId)
  {
    if (!Role.CanManageLibrary)
    {
      throw new InvalidOperationException("Only admin accounts can manage a library.");
    }

    ManagedLibraryId = libraryId;
  }

  public SavedAddress AddAddress(SavedAddressId addressId, PostalAddress address, bool isDefault)
  {
    if (_savedAddresses.Count == 0)
    {
      isDefault = true;
    }

    if (isDefault)
    {
      ClearDefaultAddress();
    }

    var savedAddress = new SavedAddress(addressId, address, isDefault);
    _savedAddresses.Add(savedAddress);
    return savedAddress;
  }

  public void UpdateAddress(SavedAddressId addressId, PostalAddress address, bool makeDefault)
  {
    var savedAddress = GetAddress(addressId);
    savedAddress.UpdateAddress(address);

    if (makeDefault)
    {
      SetDefaultAddress(addressId);
    }
  }

  public void SetDefaultAddress(SavedAddressId addressId)
  {
    var savedAddress = GetAddress(addressId);
    ClearDefaultAddress();
    savedAddress.MarkDefault();
  }

  public void RemoveAddress(SavedAddressId addressId)
  {
    var savedAddress = GetAddress(addressId);
    var removedDefault = savedAddress.IsDefault;
    _savedAddresses.Remove(savedAddress);

    if (removedDefault && _savedAddresses.Count > 0)
    {
      _savedAddresses[0].MarkDefault();
    }
  }

  private SavedAddress GetAddress(SavedAddressId addressId) =>
    _savedAddresses.FirstOrDefault(x => x.Id == addressId)
    ?? throw new InvalidOperationException($"Saved address '{addressId}' was not found.");

  private void ClearDefaultAddress()
  {
    foreach (var address in _savedAddresses.Where(x => x.IsDefault))
    {
      address.UnmarkDefault();
    }
  }
}
