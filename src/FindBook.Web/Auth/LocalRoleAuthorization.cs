using Microsoft.AspNetCore.Authorization;

namespace FindBook.Web.Auth;

public enum FindBookAccessLevel
{
  Authenticated = 0,
  UserOrHigher = 1,
  DeliveryPartnerOrHigher = 2,
  Admin = 3,
  SuperAdmin = 4
}

public sealed class FindBookAccessRequirement(FindBookAccessLevel accessLevel) : IAuthorizationRequirement
{
  public FindBookAccessLevel AccessLevel { get; } = accessLevel;
}

public sealed class FindBookAccessHandler(ICurrentAppUserAccessor currentUserAccessor)
  : AuthorizationHandler<FindBookAccessRequirement>
{
  protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, FindBookAccessRequirement requirement)
  {
    if (context.User.Identity?.IsAuthenticated != true)
    {
      return;
    }

    if (requirement.AccessLevel == FindBookAccessLevel.Authenticated)
    {
      context.Succeed(requirement);
      return;
    }

    var currentUser = await currentUserAccessor.GetCurrentUserAsync(context.User, CancellationToken.None);
    if (currentUser is null)
    {
      return;
    }

    var isAuthorized = requirement.AccessLevel switch
    {
      FindBookAccessLevel.UserOrHigher => true,
      FindBookAccessLevel.DeliveryPartnerOrHigher => currentUser.IsDeliveryPartner || currentUser.IsAdminLike,
      FindBookAccessLevel.Admin => currentUser.IsAdminLike,
      FindBookAccessLevel.SuperAdmin => currentUser.IsSuperAdmin,
      _ => false
    };

    if (isAuthorized)
    {
      context.Succeed(requirement);
    }
  }
}
