using System.Security.Claims;
using FindBook.Core.LibraryInventory.LibraryAggregate;
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
        var firebaseUidValue = principal.GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(firebaseUidValue))
        {
          return TypedResults.Unauthorized();
        }

        var firebaseUid = FirebaseUid.From(firebaseUidValue);
        var existingByFirebaseUid = await mediator.Send(new GetUserByFirebaseUidQuery(firebaseUid), cancellationToken);
        if (existingByFirebaseUid.Status == ResultStatus.Ok)
        {
          return TypedResults.Ok(MapSessionResponse(existingByFirebaseUid.Value));
        }

        var emailValue = principal.GetEmailAddress();
        if (string.IsNullOrWhiteSpace(emailValue))
        {
          return TypedResults.Unauthorized();
        }

        var email = EmailAddress.From(emailValue);
        var existingByEmail = await mediator.Send(new GetUserByEmailQuery(email), cancellationToken);
        if (existingByEmail.Status == ResultStatus.Ok)
        {
          var bound = existingByEmail.Value.FirebaseUid == firebaseUid.Value
            ? existingByEmail
            : await mediator.Send(new BindUserFirebaseUidCommand(UserAccountId.From(existingByEmail.Value.Id), firebaseUid), cancellationToken);

          return bound.ToHttpResult(user => TypedResults.Ok(MapSessionResponse(user)));
        }

        var displayName = principal.FindFirstValue(ClaimTypes.Name) ?? principal.FindFirstValue("name") ?? emailValue.Split("@")[0];
        var createResult = await mediator.Send(new CreateUserCommand(
          PersonName.From(displayName),
          email,
          PasswordHash.From("firebase-managed-account"),
          AccountRole.User,
          null,
          null,
          firebaseUid), cancellationToken);

        if (createResult.Status == ResultStatus.Conflict)
        {
          var conflicted = await mediator.Send(new GetUserByFirebaseUidQuery(firebaseUid), cancellationToken);
          if (conflicted.Status == ResultStatus.Ok)
          {
            return TypedResults.Ok(MapSessionResponse(conflicted.Value));
          }

          var conflictedByEmail = await mediator.Send(new GetUserByEmailQuery(email), cancellationToken);
          return conflictedByEmail.ToHttpResult(user => TypedResults.Ok(MapSessionResponse(user)));
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
          var firebaseUidValue = principal.GetFirebaseUid();
          if (!string.IsNullOrWhiteSpace(firebaseUidValue))
          {
            var byFirebaseUid = await mediator.Send(new GetUserByFirebaseUidQuery(FirebaseUid.From(firebaseUidValue)), cancellationToken);
            if (byFirebaseUid.Status == ResultStatus.Ok)
            {
              return TypedResults.Ok(MapSessionResponse(byFirebaseUid.Value));
            }
          }

          var emailValue = principal.GetEmailAddress();

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
    new(
      user.Id,
      user.FullName,
      user.Email,
      user.Role,
      user.ManagedLibraryId,
      BuildCapabilities(AccountRole.FromName(user.Role)));

  private static AuthSessionCapabilities BuildCapabilities(AccountRole role) =>
    new(
      role.CanAccessAdminWorkspace,
      role.CanManageCatalog,
      role.CanManageUsers,
      role.CanManageDeliveries,
      role.CanManageAdminRoles,
      role.CanViewDispatchQueue,
      role.CanOperateDeliveryTasks);
}

public sealed record AuthenticatedUserResponse(
  string? UserId,
  string? Email,
  string? Name,
  IReadOnlyCollection<AuthClaimResponse> Claims);

public sealed record AuthSessionResponse(
  int Id,
  string FullName,
  string Email,
  string Role,
  int? ManagedLibraryId,
  AuthSessionCapabilities Capabilities);

public sealed record AuthSessionCapabilities(
  bool CanAccessAdminWorkspace,
  bool CanManageCatalog,
  bool CanManageUsers,
  bool CanManageDeliveries,
  bool CanManageAdminRoles,
  bool CanViewDispatchQueue,
  bool CanOperateDeliveryTasks);

public sealed record AuthClaimResponse(string Type, string Value);
