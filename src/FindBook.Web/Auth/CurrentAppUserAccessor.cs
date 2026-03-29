using System.Security.Claims;
using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.SharedKernel;
using FindBook.Core.UserManagement.UserAccountAggregate;
using FindBook.UseCases.Users;

namespace FindBook.Web.Auth;

public interface ICurrentAppUserAccessor
{
  Task<FindBookCurrentUser?> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
}

public sealed class CurrentAppUserAccessor(IMediator mediator) : ICurrentAppUserAccessor
{
  public async Task<FindBookCurrentUser?> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
  {
    if (principal.Identity?.IsAuthenticated != true)
    {
      return null;
    }

    if (TryReadFromClaims(principal, out var currentUser))
    {
      return currentUser;
    }

    var firebaseUidValue = principal.GetFirebaseUid();
    if (!string.IsNullOrWhiteSpace(firebaseUidValue))
    {
      var byFirebaseUid = await mediator.Send(new GetUserByFirebaseUidQuery(FirebaseUid.From(firebaseUidValue)), cancellationToken);
      if (byFirebaseUid.Status == ResultStatus.Ok)
      {
        return Map(byFirebaseUid.Value);
      }
    }

    var emailValue = principal.GetEmailAddress();
    if (!string.IsNullOrWhiteSpace(emailValue))
    {
      var byEmail = await mediator.Send(new GetUserByEmailQuery(EmailAddress.From(emailValue)), cancellationToken);
      if (byEmail.Status == ResultStatus.Ok)
      {
        return Map(byEmail.Value);
      }
    }

    return null;
  }

  private static bool TryReadFromClaims(ClaimsPrincipal principal, out FindBookCurrentUser? currentUser)
  {
    currentUser = null;

    var userIdValue = principal.FindFirstValue(FindBookClaimTypes.UserId);
    var roleValue = principal.FindFirstValue(ClaimTypes.Role);
    if (!int.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(roleValue))
    {
      return false;
    }

    LibraryId? managedLibraryId = null;
    var managedLibraryValue = principal.FindFirstValue(FindBookClaimTypes.ManagedLibraryId);
    if (int.TryParse(managedLibraryValue, out var parsedManagedLibraryId))
    {
      managedLibraryId = LibraryId.From(parsedManagedLibraryId);
    }

    currentUser = new FindBookCurrentUser(
      UserAccountId.From(userId),
      principal.FindFirstValue(ClaimTypes.Name) ?? principal.FindFirstValue("name") ?? principal.FindFirstValue("email") ?? "FindBook User",
      principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email") ?? string.Empty,
      AccountRole.FromName(roleValue),
      managedLibraryId);

    return true;
  }

  private static FindBookCurrentUser Map(UserAccountDto user) =>
    new(
      UserAccountId.From(user.Id),
      user.FullName,
      user.Email,
      AccountRole.FromName(user.Role),
      user.ManagedLibraryId.HasValue ? LibraryId.From(user.ManagedLibraryId.Value) : null);
}
