using System.Security.Claims;

namespace FindBook.Web.Auth;

public static class FindBookPolicies
{
  public const string Authenticated = "Authenticated";
  public const string UserOrHigher = "UserOrHigher";
  public const string DeliveryPartnerOrHigher = "DeliveryPartnerOrHigher";
  public const string Admin = "Admin";
  public const string SuperAdmin = "SuperAdmin";
}

public static class FindBookClaimTypes
{
  public const string UserId = "findbook_user_id";
  public const string Role = ClaimTypes.Role;
  public const string ManagedLibraryId = "findbook_managed_library_id";
}
