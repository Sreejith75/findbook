using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.SharedKernel;
using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.UnitTests.Core.UserManagement;

public class UserAccountAddressBehavior
{
  [Fact]
  public void FirstAddressBecomesDefault()
  {
    var account = CreateAccount(AccountRole.User);

    var address = account.AddAddress(
      SavedAddressId.From(1),
      new PostalAddress("Street 1", "City", "State", "12345", "India"),
      isDefault: false);

    address.IsDefault.ShouldBeTrue();
    account.SavedAddresses.Count.ShouldBe(1);
  }

  [Fact]
  public void RemovingDefaultAddressPromotesAnotherAddress()
  {
    var account = CreateAccount(AccountRole.User);
    account.AddAddress(
      SavedAddressId.From(1),
      new PostalAddress("Street 1", "City", "State", "12345", "India"),
      isDefault: true);
    account.AddAddress(
      SavedAddressId.From(2),
      new PostalAddress("Street 2", "City", "State", "54321", "India"),
      isDefault: false);

    account.RemoveAddress(SavedAddressId.From(1));

    account.SavedAddresses.Count.ShouldBe(1);
    account.SavedAddresses.Single().IsDefault.ShouldBeTrue();
  }

  [Fact]
  public void NonAdminCannotManageLibrary()
  {
    var account = CreateAccount(AccountRole.User);

    Should.Throw<InvalidOperationException>(() => account.AssignManagedLibrary(LibraryId.From(7)));
  }

  private static UserAccount CreateAccount(AccountRole role) =>
    new(
      PersonName.From("Test User"),
      EmailAddress.From("user@example.com"),
      PasswordHash.From("hashed-password"),
      role,
      PhoneNumber.From("+910000000000"));
}
