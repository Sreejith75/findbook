using FindBook.Core.Feedback.BookReviewAggregate;
using FindBook.Core.Feedback.BookReviewAggregate.Specifications;
using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.LibraryInventory.BookAggregate.Specifications;
using FindBook.Core.Rental.RentalAggregate;
using FindBook.Core.Rental.RentalAggregate.Specifications;
using FindBook.Core.UserManagement.UserAccountAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate.Specifications;

namespace FindBook.UseCases.Reviews;

public sealed record BookReviewDto(int Id, int BookId, int ReviewerAccountId, int Rating, string Title, string Content, DateTimeOffset CreatedOn, DateTimeOffset? UpdatedOn);

public sealed record ListBookReviewsQuery(BookId? BookId, UserAccountId? ReviewerAccountId) : IQuery<Result<IReadOnlyCollection<BookReviewDto>>>;
public sealed record GetBookReviewByIdQuery(BookReviewId BookReviewId) : IQuery<Result<BookReviewDto>>;
public sealed record CreateBookReviewCommand(BookId BookId, UserAccountId ReviewerAccountId, StarRating Rating, ReviewTitle Title, ReviewContent Content, DateTimeOffset CreatedOn) : ICommand<Result<BookReviewDto>>;

public sealed class ListBookReviewsHandler(IReadRepository<BookReview> repository)
  : IQueryHandler<ListBookReviewsQuery, Result<IReadOnlyCollection<BookReviewDto>>>
{
  public async ValueTask<Result<IReadOnlyCollection<BookReviewDto>>> Handle(ListBookReviewsQuery query, CancellationToken cancellationToken) =>
    Result.Success<IReadOnlyCollection<BookReviewDto>>((await repository.ListAsync(new ListBookReviewsSpec(query.BookId, query.ReviewerAccountId), cancellationToken)).Select(BookReviewMappings.Map).ToArray());
}

public sealed class GetBookReviewByIdHandler(IReadRepository<BookReview> repository)
  : IQueryHandler<GetBookReviewByIdQuery, Result<BookReviewDto>>
{
  public async ValueTask<Result<BookReviewDto>> Handle(GetBookReviewByIdQuery query, CancellationToken cancellationToken)
  {
    var review = await repository.FirstOrDefaultAsync(new BookReviewByIdSpec(query.BookReviewId), cancellationToken);
    return review is null ? Result.NotFound() : Result.Success(BookReviewMappings.Map(review));
  }
}

public sealed class CreateBookReviewHandler(
  IRepository<BookReview> reviewRepository,
  IRepository<Book> bookRepository,
  IReadRepository<Rental> rentalRepository,
  IReadRepository<UserAccount> userRepository)
  : ICommandHandler<CreateBookReviewCommand, Result<BookReviewDto>>
{
  public async ValueTask<Result<BookReviewDto>> Handle(CreateBookReviewCommand command, CancellationToken cancellationToken)
  {
    var book = await bookRepository.FirstOrDefaultAsync(new BookByIdSpec(command.BookId), cancellationToken);
    if (book is null || await userRepository.FirstOrDefaultAsync(new UserAccountByIdSpec(command.ReviewerAccountId), cancellationToken) is null)
    {
      return Result.NotFound();
    }

    var duplicate = await reviewRepository.FirstOrDefaultAsync(new BookReviewByBookAndReviewerSpec(command.BookId, command.ReviewerAccountId), cancellationToken);
    if (duplicate is not null)
    {
      return Result.Conflict("A review for this book by the same user already exists.");
    }

    var completedRentalExists = (await rentalRepository.ListAsync(
      new ListRentalsSpec(command.ReviewerAccountId, command.BookId, RentalStatus.Returned),
      cancellationToken)).Count != 0;

    if (!completedRentalExists)
    {
      return Result.Error("A book can only be reviewed after it has been returned.");
    }

    var review = await reviewRepository.AddAsync(new BookReview(command.BookId, command.ReviewerAccountId, command.Rating, command.Title, command.Content, command.CreatedOn), cancellationToken);
    book.ApplyRating(command.Rating.Value);
    await bookRepository.UpdateAsync(book, cancellationToken);
    return Result.Success(BookReviewMappings.Map(review));
  }
}

internal static class BookReviewMappings
{
  public static BookReviewDto Map(BookReview review) =>
    new(review.Id.Value, review.BookId.Value, review.ReviewerAccountId.Value, review.Rating.Value, review.Title.Value, review.Content.Value, review.CreatedOn, review.UpdatedOn);
}
