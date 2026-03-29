using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace FindBook.Web.Auth;

public sealed class LocalUserClaimsTransformation(ICurrentAppUserAccessor currentUserAccessor) : IClaimsTransformation
{
  public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
  {
    if (principal.Identity?.IsAuthenticated != true)
    {
      return principal;
    }

    if (principal.HasClaim(claim => claim.Type == FindBookClaimTypes.UserId))
    {
      return principal;
    }

    var currentUser = await currentUserAccessor.GetCurrentUserAsync(principal, CancellationToken.None);
    if (currentUser is null)
    {
      return principal;
    }

    if (principal.Identity is not ClaimsIdentity identity)
    {
      return principal;
    }

    var augmentedIdentity = new ClaimsIdentity(identity);
    augmentedIdentity.AddClaim(new Claim(FindBookClaimTypes.UserId, currentUser.UserId.Value.ToString()));
    augmentedIdentity.AddClaim(new Claim(ClaimTypes.Role, currentUser.Role.Name));

    if (currentUser.ManagedLibraryId.HasValue)
    {
      augmentedIdentity.AddClaim(new Claim(FindBookClaimTypes.ManagedLibraryId, currentUser.ManagedLibraryId.Value.Value.ToString()));
    }

    return new ClaimsPrincipal(augmentedIdentity);
  }
}
