using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate.Specifications;
using FindBook.Core.SharedKernel;

namespace FindBook.UseCases.Libraries;

public sealed record AddressDto(string Street, string City, string State, string PostalCode, string Country);
public sealed record LibraryDto(int Id, string Name, string ContactEmail, string ContactPhone, AddressDto Address);

public sealed record ListLibrariesQuery() : IQuery<Result<IReadOnlyCollection<LibraryDto>>>;
public sealed record GetLibraryByIdQuery(LibraryId LibraryId) : IQuery<Result<LibraryDto>>;
public sealed record CreateLibraryCommand(LibraryName Name, PostalAddress Address, EmailAddress ContactEmail, PhoneNumber ContactPhone) : ICommand<Result<LibraryDto>>;
public sealed record UpdateLibraryCommand(LibraryId LibraryId, LibraryName Name, PostalAddress Address, EmailAddress ContactEmail, PhoneNumber ContactPhone) : ICommand<Result<LibraryDto>>;

public sealed class ListLibrariesHandler(IReadRepository<Library> repository)
  : IQueryHandler<ListLibrariesQuery, Result<IReadOnlyCollection<LibraryDto>>>
{
  public async ValueTask<Result<IReadOnlyCollection<LibraryDto>>> Handle(ListLibrariesQuery query, CancellationToken cancellationToken) =>
    Result.Success<IReadOnlyCollection<LibraryDto>>((await repository.ListAsync(new ListLibrariesSpec(), cancellationToken)).Select(LibraryMappings.Map).ToArray());
}

public sealed class GetLibraryByIdHandler(IReadRepository<Library> repository)
  : IQueryHandler<GetLibraryByIdQuery, Result<LibraryDto>>
{
  public async ValueTask<Result<LibraryDto>> Handle(GetLibraryByIdQuery query, CancellationToken cancellationToken)
  {
    var library = await repository.FirstOrDefaultAsync(new LibraryByIdSpec(query.LibraryId), cancellationToken);
    return library is null ? Result.NotFound() : Result.Success(LibraryMappings.Map(library));
  }
}

public sealed class CreateLibraryHandler(IRepository<Library> repository)
  : ICommandHandler<CreateLibraryCommand, Result<LibraryDto>>
{
  public async ValueTask<Result<LibraryDto>> Handle(CreateLibraryCommand command, CancellationToken cancellationToken)
  {
    var duplicate = await repository.FirstOrDefaultAsync(new LibraryByNameSpec(command.Name), cancellationToken);
    if (duplicate is not null) return Result.Conflict("A library with the same name already exists.");

    var created = await repository.AddAsync(new Library(command.Name, command.Address, command.ContactEmail, command.ContactPhone), cancellationToken);
    return Result.Success(LibraryMappings.Map(created));
  }
}

public sealed class UpdateLibraryHandler(IRepository<Library> repository)
  : ICommandHandler<UpdateLibraryCommand, Result<LibraryDto>>
{
  public async ValueTask<Result<LibraryDto>> Handle(UpdateLibraryCommand command, CancellationToken cancellationToken)
  {
    var library = await repository.FirstOrDefaultAsync(new LibraryByIdSpec(command.LibraryId), cancellationToken);
    if (library is null) return Result.NotFound();

    var duplicate = await repository.FirstOrDefaultAsync(new LibraryByNameSpec(command.Name), cancellationToken);
    if (duplicate is not null && duplicate.Id != library.Id) return Result.Conflict("A library with the same name already exists.");

    library.UpdateDetails(command.Name, command.Address, command.ContactEmail, command.ContactPhone);
    await repository.UpdateAsync(library, cancellationToken);
    return Result.Success(LibraryMappings.Map(library));
  }
}

internal static class LibraryMappings
{
  public static LibraryDto Map(Library library) =>
    new(
      library.Id.Value,
      library.Name.Value,
      library.ContactEmail.Value,
      library.ContactPhone.Value,
      new AddressDto(library.Address.Street, library.Address.City, library.Address.State, library.Address.PostalCode, library.Address.Country));
}
