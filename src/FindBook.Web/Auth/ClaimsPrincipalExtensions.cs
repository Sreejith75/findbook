using System.Security.Claims;

namespace FindBook.Web.Auth;

public static class ClaimsPrincipalExtensions
{
  public static string? GetFirebaseUid(this ClaimsPrincipal principal) =>
    principal.FindFirstValue("user_id")
    ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
    ?? principal.FindFirstValue("sub");

  public static string? GetEmailAddress(this ClaimsPrincipal principal) =>
    principal.FindFirstValue(ClaimTypes.Email)
    ?? principal.FindFirstValue("email");
}
