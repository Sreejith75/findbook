using FindBook.Core.Feedback.BookReviewAggregate;
using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate;
using FindBook.UseCases.Reviews;
using FindBook.Web.Api;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace FindBook.Web.Reviews;

public static class BookReviewEndpoints
{
  public static IEndpointRouteBuilder MapBookReviewEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/reviews").WithTags("Reviews");

    group.MapGet("/", async Task<HttpResult> (int? bookId, int? reviewerAccountId, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new ListBookReviewsQuery(bookId.HasValue ? BookId.From(bookId.Value) : null, reviewerAccountId.HasValue ? UserAccountId.From(reviewerAccountId.Value) : null), cancellationToken))
        .ToHttpResult(items => TypedResults.Ok(items.Select(MapResponse))));

    group.MapGet("/{id:int:min(1)}", async Task<HttpResult> (int id, IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new GetBookReviewByIdQuery(BookReviewId.From(id)), cancellationToken)).ToHttpResult(item => TypedResults.Ok(MapResponse(item))));

    group.MapPost("/", async Task<HttpResult> (CreateBookReviewRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var errors = new ValidationErrorBuilder();
      ApiValidation.TryCreate(() => StarRating.From(request.Rating), nameof(request.Rating), errors, out var rating);
      ApiValidation.TryCreate(() => ReviewTitle.From(request.Title), nameof(request.Title), errors, out var title);
      ApiValidation.TryCreate(() => ReviewContent.From(request.Content), nameof(request.Content), errors, out var content);
      if (errors.HasErrors) return ApiValidation.ValidationProblem(errors);

      var result = await mediator.Send(new CreateBookReviewCommand(BookId.From(request.BookId), UserAccountId.From(request.ReviewerAccountId), rating, title, content, request.CreatedOn), cancellationToken);
      return result.ToHttpResult(item => TypedResults.Created($"/api/reviews/{item.Id}", MapResponse(item)));
    });

    return app;
  }

  private static BookReviewResponse MapResponse(BookReviewDto review) =>
    new(review.Id, review.BookId, review.ReviewerAccountId, review.Rating, review.Title, review.Content, review.CreatedOn, review.UpdatedOn);
}

public sealed record CreateBookReviewRequest(int BookId, int ReviewerAccountId, int Rating, string Title, string Content, DateTimeOffset CreatedOn);
public sealed record BookReviewResponse(int Id, int BookId, int ReviewerAccountId, int Rating, string Title, string Content, DateTimeOffset CreatedOn, DateTimeOffset? UpdatedOn);
