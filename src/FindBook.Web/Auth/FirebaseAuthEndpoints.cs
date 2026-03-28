using System.Security.Claims;
using FindBook.Core.SharedKernel;
using FindBook.Core.UserManagement.UserAccountAggregate;
using FindBook.UseCases.Users;
using FindBook.Web.Api;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace FindBook.Web.Auth;

public static class FirebaseAuthEndpoints
{
  public static IEndpointRouteBuilder MapFirebaseAuthEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/auth")
      .WithTags("Authentication");

    group.MapGet("/me", (ClaimsPrincipal user) =>
      {
        var claims = user.Claims
          .Select(claim => new AuthClaimResponse(claim.Type, claim.Value))
          .ToArray();

        return TypedResults.Ok(new AuthenticatedUserResponse(
          user.FindFirstValue("user_id") ?? user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub"),
          user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email"),
          user.FindFirstValue(ClaimTypes.Name) ?? user.FindFirstValue("name"),
          claims));
      })
      .RequireAuthorization()
      .WithSummary("Returns the authenticated Firebase user and claims");

    group.MapGet("/session", async Task<HttpResult> (ClaimsPrincipal principal, IMediator mediator, CancellationToken cancellationToken) =>
      {
        var emailValue = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email");
        if (string.IsNullOrWhiteSpace(emailValue))
        {
          return TypedResults.Unauthorized();
        }

        var email = EmailAddress.From(emailValue);
        var existing = await mediator.Send(new GetUserByEmailQuery(email), cancellationToken);
        if (existing.Status == ResultStatus.Ok)
        {
          return TypedResults.Ok(MapSessionResponse(existing.Value));
        }

        var displayName = principal.FindFirstValue(ClaimTypes.Name) ?? principal.FindFirstValue("name") ?? emailValue.Split("@")[0];
        var createResult = await mediator.Send(new CreateUserCommand(
          PersonName.From(displayName),
          email,
          PasswordHash.From("firebase-managed-account"),
          AccountRole.User,
          null,
          null), cancellationToken);

        if (createResult.Status == ResultStatus.Conflict)
        {
          var conflicted = await mediator.Send(new GetUserByEmailQuery(email), cancellationToken);
          return conflicted.ToHttpResult(user => TypedResults.Ok(MapSessionResponse(user)));
        }

        return createResult.ToHttpResult(user => TypedResults.Ok(MapSessionResponse(user)));
      })
      .AddEndpointFilter(async (context, next) =>
      {
        try
        {
          return await next(context);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
          var principal = context.HttpContext.User;
          var mediator = context.HttpContext.RequestServices.GetRequiredService<IMediator>();
          var cancellationToken = context.HttpContext.RequestAborted;
          var emailValue = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email");

          if (string.IsNullOrWhiteSpace(emailValue))
          {
            throw;
          }

          var email = EmailAddress.From(emailValue);
          var existing = await mediator.Send(new GetUserByEmailQuery(email), cancellationToken);
          return existing.ToHttpResult(user => TypedResults.Ok(MapSessionResponse(user)));
        }
      })
      .RequireAuthorization()
      .WithSummary("Resolves the authenticated Firebase identity to a local FindBook user");

    return app;
  }

  private static AuthSessionResponse MapSessionResponse(UserAccountDto user) =>
    new(user.Id, user.FullName, user.Email, user.Role);
}

public sealed record AuthenticatedUserResponse(
  string? UserId,
  string? Email,
  string? Name,
  IReadOnlyCollection<AuthClaimResponse> Claims);

public sealed record AuthSessionResponse(int Id, string FullName, string Email, string Role);

public sealed record AuthClaimResponse(string Type, string Value);
