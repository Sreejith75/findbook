using FindBook.Core.LibraryInventory.CategoryAggregate;
using FindBook.UseCases.Categories;
using FindBook.Web.Api;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace FindBook.Web.Categories;

public static class CategoryEndpoints
{
  public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/categories").WithTags("Categories");

    group.MapGet("/", async Task<HttpResult> (IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new ListCategoriesQuery(), cancellationToken)).ToHttpResult(items => TypedResults.Ok(items.Select(MapResponse))));

    group.MapGet("/{id:int:min(1)}", async Task<HttpResult> (int id, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new GetCategoryByIdQuery(CategoryId.From(id)), cancellationToken)).ToHttpResult(item => TypedResults.Ok(MapResponse(item))));

    group.MapPost("/", async Task<HttpResult> (UpsertCategoryRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var errors = new ValidationErrorBuilder();
      ApiValidation.TryCreate(() => CategoryName.From(request.Name), nameof(request.Name), errors, out var name);
      if (errors.HasErrors) return ApiValidation.ValidationProblem(errors);

      var result = await mediator.Send(new CreateCategoryCommand(name), cancellationToken);
      return result.ToHttpResult(item => TypedResults.Created($"/api/categories/{item.Id}", MapResponse(item)));
    });

    group.MapPut("/{id:int:min(1)}", async Task<HttpResult> (int id, UpsertCategoryRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var errors = new ValidationErrorBuilder();
      ApiValidation.TryCreate(() => CategoryName.From(request.Name), nameof(request.Name), errors, out var name);
      if (errors.HasErrors) return ApiValidation.ValidationProblem(errors);

      var result = await mediator.Send(new UpdateCategoryCommand(CategoryId.From(id), name), cancellationToken);
      return result.ToHttpResult(item => TypedResults.Ok(MapResponse(item)));
    });

    return app;
  }

  private static CategoryResponse MapResponse(CategoryDto category) => new(category.Id, category.Name);
}

public sealed record UpsertCategoryRequest(string Name);
public sealed record CategoryResponse(int Id, string Name);
