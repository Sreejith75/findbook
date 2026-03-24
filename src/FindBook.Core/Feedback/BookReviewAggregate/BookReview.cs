using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.Core.Feedback.BookReviewAggregate;

public class BookReview : EntityBase<BookReview, BookReviewId>, IAggregateRoot
{
  private BookReview() { }

  public BookReview(
    BookId bookId,
    UserAccountId reviewerAccountId,
    StarRating rating,
    ReviewTitle title,
    ReviewContent content,
    DateTimeOffset createdOn)
  {
    BookId = bookId;
    ReviewerAccountId = reviewerAccountId;
    Rating = rating;
    Title = title;
    Content = content;
    CreatedOn = createdOn;
  }

  public BookId BookId { get; private set; }
  public UserAccountId ReviewerAccountId { get; private set; }
  public StarRating Rating { get; private set; }
  public ReviewTitle Title { get; private set; }
  public ReviewContent Content { get; private set; }
  public DateTimeOffset CreatedOn { get; private set; }
  public DateTimeOffset? UpdatedOn { get; private set; }

  public void Update(StarRating rating, ReviewTitle title, ReviewContent content, DateTimeOffset updatedOn)
  {
    Rating = rating;
    Title = title;
    Content = content;
    UpdatedOn = updatedOn;
  }
}
