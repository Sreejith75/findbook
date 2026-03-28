using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate.Specifications;
using FindBook.Core.SharedKernel;
using FindBook.Core.UserManagement.UserAccountAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate.Specifications;

namespace FindBook.UseCases.Users;

public sealed record SavedAddressDto(
  int Id,
  string Street,
  string City,
  string State,
  string PostalCode,
  string Country,
  bool IsDefault);

public sealed record UserAccountDto(
  int Id,
  string FullName,
  string Email,
  string? PhoneNumber,
  string Role,
  int? ManagedLibraryId,
  IReadOnlyCollection<SavedAddressDto> Addresses);

public sealed record ListUsersQuery() : IQuery<Result<IReadOnlyCollection<UserAccountDto>>>;
public sealed record GetUserByIdQuery(UserAccountId UserId) : IQuery<Result<UserAccountDto>>;
public sealed record GetUserByEmailQuery(EmailAddress Email) : IQuery<Result<UserAccountDto>>;
public sealed record CreateUserCommand(PersonName FullName, EmailAddress Email, PasswordHash PasswordHash, AccountRole Role, PhoneNumber? PhoneNumber, LibraryId? ManagedLibraryId) : ICommand<Result<UserAccountDto>>;
public sealed record UpdateUserCommand(UserAccountId UserId, PersonName FullName, EmailAddress Email, AccountRole Role, PhoneNumber? PhoneNumber, LibraryId? ManagedLibraryId) : ICommand<Result<UserAccountDto>>;
public sealed record AddUserAddressCommand(UserAccountId UserId, PostalAddress Address, bool IsDefault) : ICommand<Result<UserAccountDto>>;
public sealed record UpdateUserAddressCommand(UserAccountId UserId, SavedAddressId AddressId, PostalAddress Address, bool IsDefault) : ICommand<Result<UserAccountDto>>;
public sealed record SetDefaultUserAddressCommand(UserAccountId UserId, SavedAddressId AddressId) : ICommand<Result<UserAccountDto>>;
public sealed record DeleteUserAddressCommand(UserAccountId UserId, SavedAddressId AddressId) : ICommand<Result>;

public sealed class ListUsersHandler(IReadRepository<UserAccount> repository)
  : IQueryHandler<ListUsersQuery, Result<IReadOnlyCollection<UserAccountDto>>>
{
  public async ValueTask<Result<IReadOnlyCollection<UserAccountDto>>> Handle(ListUsersQuery query, CancellationToken cancellationToken)
  {
    var users = await repository.ListAsync(new ListUserAccountsSpec(), cancellationToken);
    return Result.Success<IReadOnlyCollection<UserAccountDto>>(users.Select(UserAccountMappings.Map).ToArray());
  }
}

public sealed class GetUserByIdHandler(IReadRepository<UserAccount> repository)
  : IQueryHandler<GetUserByIdQuery, Result<UserAccountDto>>
{
  public async ValueTask<Result<UserAccountDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
  {
    var user = await repository.FirstOrDefaultAsync(new UserAccountByIdSpec(query.UserId), cancellationToken);
    return user is null ? Result.NotFound() : Result.Success(UserAccountMappings.Map(user));
  }
}

public sealed class GetUserByEmailHandler(IReadRepository<UserAccount> repository)
  : IQueryHandler<GetUserByEmailQuery, Result<UserAccountDto>>
{
  public async ValueTask<Result<UserAccountDto>> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken)
  {
    var user = await repository.FirstOrDefaultAsync(new UserAccountByEmailSpec(query.Email), cancellationToken);
    return user is null ? Result.NotFound() : Result.Success(UserAccountMappings.Map(user));
  }
}

public sealed class CreateUserHandler(
  IRepository<UserAccount> userRepository,
  IReadRepository<Library> libraryRepository)
  : ICommandHandler<CreateUserCommand, Result<UserAccountDto>>
{
  public async ValueTask<Result<UserAccountDto>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
  {
    var duplicate = await userRepository.FirstOrDefaultAsync(new UserAccountByEmailSpec(command.Email), cancellationToken);
    if (duplicate is not null)
    {
      return Result.Conflict("A user with the same email already exists.");
    }

    if (command.ManagedLibraryId.HasValue)
    {
      if (!command.Role.CanManageLibrary)
      {
        return Result.Invalid(new ValidationError { Identifier = nameof(command.ManagedLibraryId), ErrorMessage = "Only admin-capable roles can manage a library." });
      }

      var library = await libraryRepository.FirstOrDefaultAsync(new LibraryByIdSpec(command.ManagedLibraryId.Value), cancellationToken);
      if (library is null)
      {
        return Result.NotFound();
      }
    }

    var user = new UserAccount(command.FullName, command.Email, command.PasswordHash, command.Role, command.PhoneNumber);
    if (command.ManagedLibraryId.HasValue)
    {
      user.AssignManagedLibrary(command.ManagedLibraryId.Value);
    }

    var created = await userRepository.AddAsync(user, cancellationToken);
    return Result.Success(UserAccountMappings.Map(created));
  }
}

public sealed class UpdateUserHandler(
  IRepository<UserAccount> userRepository,
  IReadRepository<Library> libraryRepository)
  : ICommandHandler<UpdateUserCommand, Result<UserAccountDto>>
{
  public async ValueTask<Result<UserAccountDto>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
  {
    var user = await userRepository.FirstOrDefaultAsync(new UserAccountByIdSpec(command.UserId), cancellationToken);
    if (user is null)
    {
      return Result.NotFound();
    }

    var duplicate = await userRepository.FirstOrDefaultAsync(new UserAccountByEmailSpec(command.Email), cancellationToken);
    if (duplicate is not null && duplicate.Id != user.Id)
    {
      return Result.Conflict("A user with the same email already exists.");
    }

    user.UpdateProfile(command.FullName, command.Email, command.PhoneNumber);
    user.ChangeRole(command.Role);

    if (command.ManagedLibraryId.HasValue)
    {
      if (!command.Role.CanManageLibrary)
      {
        return Result.Invalid(new ValidationError { Identifier = nameof(command.ManagedLibraryId), ErrorMessage = "Only admin-capable roles can manage a library." });
      }

      var library = await libraryRepository.FirstOrDefaultAsync(new LibraryByIdSpec(command.ManagedLibraryId.Value), cancellationToken);
      if (library is null)
      {
        return Result.NotFound();
      }

      user.AssignManagedLibrary(command.ManagedLibraryId.Value);
    }

    await userRepository.UpdateAsync(user, cancellationToken);
    return Result.Success(UserAccountMappings.Map(user));
  }
}

public sealed class AddUserAddressHandler(IRepository<UserAccount> repository)
  : ICommandHandler<AddUserAddressCommand, Result<UserAccountDto>>
{
  public async ValueTask<Result<UserAccountDto>> Handle(AddUserAddressCommand command, CancellationToken cancellationToken)
  {
    var user = await repository.FirstOrDefaultAsync(new UserAccountByIdSpec(command.UserId), cancellationToken);
    if (user is null)
    {
      return Result.NotFound();
    }

    var nextId = SavedAddressId.From(user.SavedAddresses.Any() ? user.SavedAddresses.Max(x => x.Id.Value) + 1 : 1);
    user.AddAddress(nextId, command.Address, command.IsDefault);
    await repository.UpdateAsync(user, cancellationToken);
    return Result.Success(UserAccountMappings.Map(user));
  }
}

public sealed class UpdateUserAddressHandler(IRepository<UserAccount> repository)
  : ICommandHandler<UpdateUserAddressCommand, Result<UserAccountDto>>
{
  public async ValueTask<Result<UserAccountDto>> Handle(UpdateUserAddressCommand command, CancellationToken cancellationToken)
  {
    var user = await repository.FirstOrDefaultAsync(new UserAccountByIdSpec(command.UserId), cancellationToken);
    if (user is null)
    {
      return Result.NotFound();
    }

    try
    {
      user.UpdateAddress(command.AddressId, command.Address, command.IsDefault);
      await repository.UpdateAsync(user, cancellationToken);
      return Result.Success(UserAccountMappings.Map(user));
    }
    catch (InvalidOperationException ex)
    {
      return Result.Error(ex.Message);
    }
  }
}

public sealed class SetDefaultUserAddressHandler(IRepository<UserAccount> repository)
  : ICommandHandler<SetDefaultUserAddressCommand, Result<UserAccountDto>>
{
  public async ValueTask<Result<UserAccountDto>> Handle(SetDefaultUserAddressCommand command, CancellationToken cancellationToken)
  {
    var user = await repository.FirstOrDefaultAsync(new UserAccountByIdSpec(command.UserId), cancellationToken);
    if (user is null)
    {
      return Result.NotFound();
    }

    try
    {
      user.SetDefaultAddress(command.AddressId);
      await repository.UpdateAsync(user, cancellationToken);
      return Result.Success(UserAccountMappings.Map(user));
    }
    catch (InvalidOperationException ex)
    {
      return Result.Error(ex.Message);
    }
  }
}

public sealed class DeleteUserAddressHandler(IRepository<UserAccount> repository)
  : ICommandHandler<DeleteUserAddressCommand, Result>
{
  public async ValueTask<Result> Handle(DeleteUserAddressCommand command, CancellationToken cancellationToken)
  {
    var user = await repository.FirstOrDefaultAsync(new UserAccountByIdSpec(command.UserId), cancellationToken);
    if (user is null)
    {
      return Result.NotFound();
    }

    try
    {
      user.RemoveAddress(command.AddressId);
      await repository.UpdateAsync(user, cancellationToken);
      return Result.Success();
    }
    catch (InvalidOperationException ex)
    {
      return Result.Error(ex.Message);
    }
  }
}

internal static class UserAccountMappings
{
  public static UserAccountDto Map(UserAccount user) =>
    new(
      user.Id.Value,
      user.FullName.Value,
      user.Email.Value,
      user.PhoneNumber?.Value,
      user.Role.Name,
      user.ManagedLibraryId?.Value,
      user.SavedAddresses
        .OrderByDescending(x => x.IsDefault)
        .ThenBy(x => x.Id.Value)
        .Select(x => new SavedAddressDto(
          x.Id.Value,
          x.Address.Street,
          x.Address.City,
          x.Address.State,
          x.Address.PostalCode,
          x.Address.Country,
          x.IsDefault))
        .ToArray());
}
