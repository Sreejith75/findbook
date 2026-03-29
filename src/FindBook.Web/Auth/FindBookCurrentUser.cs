using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.Web.Auth;

public sealed record FindBookCurrentUser(
  UserAccountId UserId,
  string FullName,
  string Email,
  AccountRole Role,
  LibraryId? ManagedLibraryId)
{
  public bool IsAdminLike => Role.IsAdminLike;
  public bool IsSuperAdmin => Role == AccountRole.SuperAdmin;
  public bool IsDeliveryPartner => Role == AccountRole.DeliveryPartner;
}
