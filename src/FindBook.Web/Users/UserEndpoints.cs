using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.SharedKernel;
using FindBook.Core.UserManagement.UserAccountAggregate;
using FindBook.UseCases.Users;
using FindBook.Web.Api;
using FindBook.Web.Auth;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace FindBook.Web.Users;

public static class UserEndpoints
{
  public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization(FindBookPolicies.Admin);
    var meGroup = app.MapGroup("/api/me").WithTags("Profile").RequireAuthorization(FindBookPolicies.Authenticated);

    group.MapGet("/", async Task<HttpResult> (IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new ListUsersQuery(), cancellationToken)).ToHttpResult(users => TypedResults.Ok(users.Select(MapUserResponse))));

    group.MapGet("/{id:int:min(1)}", async Task<HttpResult> (int id, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new GetUserByIdQuery(UserAccountId.From(id)), cancellationToken)).ToHttpResult(user => TypedResults.Ok(MapUserResponse(user))));

    group.MapPost("/", async Task<HttpResult> (HttpContext httpContext, CreateUserRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var currentUser = await httpContext.GetCurrentFindBookUserAsync(cancellationToken);
      if (currentUser is null)
      {
        return TypedResults.Forbid();
      }

      var errors = new ValidationErrorBuilder();
      ApiValidation.TryCreate(() => PersonName.From(request.FullName), nameof(request.FullName), errors, out var fullName);
      ApiValidation.TryCreate(() => EmailAddress.From(request.Email), nameof(request.Email), errors, out var email);
      ApiValidation.TryCreate(() => PasswordHash.From(request.PasswordHash), nameof(request.PasswordHash), errors, out var passwordHash);

      PhoneNumber? phoneNumber = null;
      if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
      {
        ApiValidation.TryCreate(() => PhoneNumber.From(request.PhoneNumber), nameof(request.PhoneNumber), errors, out PhoneNumber parsedPhoneNumber);
        phoneNumber = parsedPhoneNumber;
      }

      ApiValidation.TryParseSmartEnum(request.Role, nameof(request.Role), errors, out AccountRole role);
      if (errors.HasErrors) return ApiValidation.ValidationProblem(errors);
      if (!CanManageRole(currentUser, role))
      {
        return TypedResults.Forbid();
      }

      var result = await mediator.Send(new CreateUserCommand(fullName, email, passwordHash, role, phoneNumber, request.ManagedLibraryId.HasValue ? LibraryId.From(request.ManagedLibraryId.Value) : null), cancellationToken);
      return result.ToHttpResult(user => TypedResults.Created($"/api/users/{user.Id}", MapUserResponse(user)));
    });

    group.MapPut("/{id:int:min(1)}", async Task<HttpResult> (HttpContext httpContext, int id, UpdateUserRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var currentUser = await httpContext.GetCurrentFindBookUserAsync(cancellationToken);
      if (currentUser is null)
      {
        return TypedResults.Forbid();
      }

      var existingUserResult = await mediator.Send(new GetUserByIdQuery(UserAccountId.From(id)), cancellationToken);
      if (existingUserResult.Status != ResultStatus.Ok)
      {
        return existingUserResult.ToHttpResult(user => TypedResults.Ok(MapUserResponse(user)));
      }

      var errors = new ValidationErrorBuilder();
      ApiValidation.TryCreate(() => PersonName.From(request.FullName), nameof(request.FullName), errors, out var fullName);
      ApiValidation.TryCreate(() => EmailAddress.From(request.Email), nameof(request.Email), errors, out var email);

      PhoneNumber? phoneNumber = null;
      if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
      {
        ApiValidation.TryCreate(() => PhoneNumber.From(request.PhoneNumber), nameof(request.PhoneNumber), errors, out PhoneNumber parsedPhoneNumber);
        phoneNumber = parsedPhoneNumber;
      }

      ApiValidation.TryParseSmartEnum(request.Role, nameof(request.Role), errors, out AccountRole role);
      if (errors.HasErrors) return ApiValidation.ValidationProblem(errors);
      if (!CanManageRole(currentUser, role) || !CanManageRole(currentUser, AccountRole.FromName(existingUserResult.Value.Role)))
      {
        return TypedResults.Forbid();
      }

      var result = await mediator.Send(new UpdateUserCommand(UserAccountId.From(id), fullName, email, role, phoneNumber, request.ManagedLibraryId.HasValue ? LibraryId.From(request.ManagedLibraryId.Value) : null), cancellationToken);
      return result.ToHttpResult(user => TypedResults.Ok(MapUserResponse(user)));
    });

    meGroup.MapGet("/", async Task<HttpResult> (HttpContext httpContext, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var currentUser = await httpContext.GetCurrentFindBookUserAsync(cancellationToken);
      if (currentUser is null)
      {
        return TypedResults.Forbid();
      }

      return (await mediator.Send(new GetUserByIdQuery(currentUser.UserId), cancellationToken))
        .ToHttpResult(user => TypedResults.Ok(MapUserResponse(user)));
    });

    meGroup.MapPut("/", async Task<HttpResult> (HttpContext httpContext, UpdateMyProfileRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var currentUser = await httpContext.GetCurrentFindBookUserAsync(cancellationToken);
      if (currentUser is null)
      {
        return TypedResults.Forbid();
      }

      var errors = new ValidationErrorBuilder();
      ApiValidation.TryCreate(() => PersonName.From(request.FullName), nameof(request.FullName), errors, out var fullName);
      ApiValidation.TryCreate(() => EmailAddress.From(request.Email), nameof(request.Email), errors, out var email);

      PhoneNumber? phoneNumber = null;
      if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
      {
        ApiValidation.TryCreate(() => PhoneNumber.From(request.PhoneNumber), nameof(request.PhoneNumber), errors, out PhoneNumber parsedPhoneNumber);
        phoneNumber = parsedPhoneNumber;
      }

      if (errors.HasErrors) return ApiValidation.ValidationProblem(errors);

      var result = await mediator.Send(new UpdateUserCommand(currentUser.UserId, fullName, email, currentUser.Role, phoneNumber, currentUser.ManagedLibraryId), cancellationToken);
      return result.ToHttpResult(user => TypedResults.Ok(MapUserResponse(user)));
    });

    meGroup.MapPost("/addresses", async Task<HttpResult> (HttpContext httpContext, AddressContract request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var currentUser = await httpContext.GetCurrentFindBookUserAsync(cancellationToken);
      if (currentUser is null)
      {
        return TypedResults.Forbid();
      }

      var errors = new ValidationErrorBuilder();
      ApiValidation.TryCreate(() => new PostalAddress(request.Street, request.City, request.State, request.PostalCode, request.Country), "address", errors, out var address);
      if (errors.HasErrors) return ApiValidation.ValidationProblem(errors);

      var result = await mediator.Send(new AddUserAddressCommand(currentUser.UserId, address, request.IsDefault), cancellationToken);
      return result.ToHttpResult(user => TypedResults.Created("/api/me", MapUserResponse(user)));
    });

    meGroup.MapPut("/addresses/{addressId:int:min(1)}", async Task<HttpResult> (HttpContext httpContext, int addressId, AddressContract request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var currentUser = await httpContext.GetCurrentFindBookUserAsync(cancellationToken);
      if (currentUser is null)
      {
        return TypedResults.Forbid();
      }

      var errors = new ValidationErrorBuilder();
      ApiValidation.TryCreate(() => new PostalAddress(request.Street, request.City, request.State, request.PostalCode, request.Country), "address", errors, out var address);
      if (errors.HasErrors) return ApiValidation.ValidationProblem(errors);

      var result = await mediator.Send(new UpdateUserAddressCommand(currentUser.UserId, SavedAddressId.From(addressId), address, request.IsDefault), cancellationToken);
      return result.ToHttpResult(user => TypedResults.Ok(MapUserResponse(user)));
    });

    meGroup.MapPost("/addresses/{addressId:int:min(1)}/default", async Task<HttpResult> (HttpContext httpContext, int addressId, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var currentUser = await httpContext.GetCurrentFindBookUserAsync(cancellationToken);
      if (currentUser is null)
      {
        return TypedResults.Forbid();
      }

      return (await mediator.Send(new SetDefaultUserAddressCommand(currentUser.UserId, SavedAddressId.From(addressId)), cancellationToken))
        .ToHttpResult(user => TypedResults.Ok(MapUserResponse(user)));
    });

    meGroup.MapDelete("/addresses/{addressId:int:min(1)}", async Task<HttpResult> (HttpContext httpContext, int addressId, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var currentUser = await httpContext.GetCurrentFindBookUserAsync(cancellationToken);
      if (currentUser is null)
      {
        return TypedResults.Forbid();
      }

      return (await mediator.Send(new DeleteUserAddressCommand(currentUser.UserId, SavedAddressId.From(addressId)), cancellationToken))
        .ToHttpResult(() => TypedResults.NoContent());
    });

    return app;
  }

  private static bool CanManageRole(FindBookCurrentUser currentUser, AccountRole targetRole) =>
    !targetRole.IsAdminLike || currentUser.Role.CanManageAdminRoles;

  private static UserResponse MapUserResponse(UserAccountDto user) =>
    new(
      user.Id,
      user.FullName,
      user.Email,
      user.PhoneNumber,
      user.Role,
      user.ManagedLibraryId,
      user.Addresses.Select(address => new SavedAddressResponse(address.Id, address.Street, address.City, address.State, address.PostalCode, address.Country, address.IsDefault)).ToArray());
}

public sealed record CreateUserRequest(string FullName, string Email, string PasswordHash, string Role, string? PhoneNumber, int? ManagedLibraryId);
public sealed record UpdateUserRequest(string FullName, string Email, string Role, string? PhoneNumber, int? ManagedLibraryId);
public sealed record UpdateMyProfileRequest(string FullName, string Email, string? PhoneNumber);
public sealed record UserResponse(int Id, string FullName, string Email, string? PhoneNumber, string Role, int? ManagedLibraryId, IReadOnlyCollection<SavedAddressResponse> Addresses);
public sealed record SavedAddressResponse(int Id, string Street, string City, string State, string PostalCode, string Country, bool IsDefault);
