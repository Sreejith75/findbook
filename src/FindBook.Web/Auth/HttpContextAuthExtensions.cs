using Microsoft.AspNetCore.Http.HttpResults;

namespace FindBook.Web.Auth;

public static class HttpContextAuthExtensions
{
  public static async Task<FindBookCurrentUser?> GetCurrentFindBookUserAsync(this HttpContext httpContext, CancellationToken cancellationToken)
  {
    var accessor = httpContext.RequestServices.GetRequiredService<ICurrentAppUserAccessor>();
    return await accessor.GetCurrentUserAsync(httpContext.User, cancellationToken);
  }

  public static async Task<Results<UnauthorizedHttpResult, ForbidHttpResult, Ok<FindBookCurrentUser>>> RequireCurrentFindBookUserAsync(this HttpContext httpContext, CancellationToken cancellationToken)
  {
    if (httpContext.User.Identity?.IsAuthenticated != true)
    {
      return TypedResults.Unauthorized();
    }

    var currentUser = await httpContext.GetCurrentFindBookUserAsync(cancellationToken);
    return currentUser is null ? TypedResults.Forbid() : TypedResults.Ok(currentUser);
  }
}
