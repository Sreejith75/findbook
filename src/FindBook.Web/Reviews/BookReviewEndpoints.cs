using FindBook.Core.Feedback.BookReviewAggregate;
using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate;
using FindBook.UseCases.Reviews;
using FindBook.Web.Api;
using FindBook.Web.Auth;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace FindBook.Web.Reviews;

public static class BookReviewEndpoints
{
  public static IEndpointRouteBuilder MapBookReviewEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/reviews").WithTags("Reviews").RequireAuthorization(FindBookPolicies.Authenticated);

    group.MapGet("/", async Task<HttpResult> (HttpContext httpContext, int? bookId, int? reviewerAccountId, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var currentUser = await httpContext.GetCurrentFindBookUserAsync(cancellationToken);
      if (currentUser is null)
      {
        return TypedResults.Forbid();
      }

      UserAccountId? effectiveReviewerId = currentUser.IsAdminLike
        ? reviewerAccountId.HasValue ? UserAccountId.From(reviewerAccountId.Value) : null
        : currentUser.UserId;

      return (await mediator.Send(new ListBookReviewsQuery(bookId.HasValue ? BookId.From(bookId.Value) : null, effectiveReviewerId), cancellationToken))
        .ToHttpResult(items => TypedResults.Ok(items.Select(MapResponse)));
    });

    group.MapGet("/{id:int:min(1)}", async Task<HttpResult> (HttpContext httpContext, int id, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var currentUser = await httpContext.GetCurrentFindBookUserAsync(cancellationToken);
      if (currentUser is null)
      {
        return TypedResults.Forbid();
      }

      var result = await mediator.Send(new GetBookReviewByIdQuery(BookReviewId.From(id)), cancellationToken);
      if (result.Status != ResultStatus.Ok)
      {
        return result.ToHttpResult(item => TypedResults.Ok(MapResponse(item)));
      }

      if (!currentUser.IsAdminLike && result.Value.ReviewerAccountId != currentUser.UserId.Value)
      {
        return TypedResults.Forbid();
      }

      return TypedResults.Ok(MapResponse(result.Value));
    });

    group.MapPost("/", async Task<HttpResult> (HttpContext httpContext, CreateBookReviewRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
      var currentUser = await httpContext.GetCurrentFindBookUserAsync(cancellationToken);
      if (currentUser is null || currentUser.IsDeliveryPartner)
      {
        return TypedResults.Forbid();
      }

      var errors = new ValidationErrorBuilder();
      ApiValidation.TryCreate(() => StarRating.From(request.Rating), nameof(request.Rating), errors, out var rating);
      ApiValidation.TryCreate(() => ReviewTitle.From(request.Title), nameof(request.Title), errors, out var title);
      ApiValidation.TryCreate(() => ReviewContent.From(request.Content), nameof(request.Content), errors, out var content);
      if (errors.HasErrors)
      {
        return ApiValidation.ValidationProblem(errors);
      }

      var result = await mediator.Send(new CreateBookReviewCommand(BookId.From(request.BookId), currentUser.UserId, rating, title, content, request.CreatedOn), cancellationToken);
      return result.ToHttpResult(item => TypedResults.Created($"/api/reviews/{item.Id}", MapResponse(item)));
    });

    return app;
  }

  private static BookReviewResponse MapResponse(BookReviewDto review) =>
    new(review.Id, review.BookId, review.ReviewerAccountId, review.Rating, review.Title, review.Content, review.CreatedOn, review.UpdatedOn);
}

public sealed record CreateBookReviewRequest(int BookId, int Rating, string Title, string Content, DateTimeOffset CreatedOn);
public sealed record BookReviewResponse(int Id, int BookId, int ReviewerAccountId, int Rating, string Title, string Content, DateTimeOffset CreatedOn, DateTimeOffset? UpdatedOn);
