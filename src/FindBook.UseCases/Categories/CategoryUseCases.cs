using FindBook.Core.LibraryInventory.CategoryAggregate;
using FindBook.Core.LibraryInventory.CategoryAggregate.Specifications;

namespace FindBook.UseCases.Categories;

public sealed record CategoryDto(int Id, string Name);
public sealed record ListCategoriesQuery() : IQuery<Result<IReadOnlyCollection<CategoryDto>>>;
public sealed record GetCategoryByIdQuery(CategoryId CategoryId) : IQuery<Result<CategoryDto>>;
public sealed record CreateCategoryCommand(CategoryName Name) : ICommand<Result<CategoryDto>>;
public sealed record UpdateCategoryCommand(CategoryId CategoryId, CategoryName Name) : ICommand<Result<CategoryDto>>;

public sealed class ListCategoriesHandler(IReadRepository<Category> repository)
  : IQueryHandler<ListCategoriesQuery, Result<IReadOnlyCollection<CategoryDto>>>
{
  public async ValueTask<Result<IReadOnlyCollection<CategoryDto>>> Handle(ListCategoriesQuery query, CancellationToken cancellationToken) =>
    Result.Success<IReadOnlyCollection<CategoryDto>>((await repository.ListAsync(new ListCategoriesSpec(), cancellationToken)).Select(CategoryMappings.Map).ToArray());
}

public sealed class GetCategoryByIdHandler(IReadRepository<Category> repository)
  : IQueryHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
  public async ValueTask<Result<CategoryDto>> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
  {
    var category = await repository.FirstOrDefaultAsync(new CategoryByIdSpec(query.CategoryId), cancellationToken);
    return category is null ? Result.NotFound() : Result.Success(CategoryMappings.Map(category));
  }
}

public sealed class CreateCategoryHandler(IRepository<Category> repository)
  : ICommandHandler<CreateCategoryCommand, Result<CategoryDto>>
{
  public async ValueTask<Result<CategoryDto>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
  {
    var duplicate = await repository.FirstOrDefaultAsync(new CategoryByNameSpec(command.Name), cancellationToken);
    if (duplicate is not null) return Result.Conflict("A category with the same name already exists.");

    var created = await repository.AddAsync(new Category(command.Name), cancellationToken);
    return Result.Success(CategoryMappings.Map(created));
  }
}

public sealed class UpdateCategoryHandler(IRepository<Category> repository)
  : ICommandHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
  public async ValueTask<Result<CategoryDto>> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
  {
    var category = await repository.FirstOrDefaultAsync(new CategoryByIdSpec(command.CategoryId), cancellationToken);
    if (category is null) return Result.NotFound();

    var duplicate = await repository.FirstOrDefaultAsync(new CategoryByNameSpec(command.Name), cancellationToken);
    if (duplicate is not null && duplicate.Id != category.Id) return Result.Conflict("A category with the same name already exists.");

    category.Rename(command.Name);
    await repository.UpdateAsync(category, cancellationToken);
    return Result.Success(CategoryMappings.Map(category));
  }
}

internal static class CategoryMappings
{
  public static CategoryDto Map(Category category) => new(category.Id.Value, category.Name.Value);
}
