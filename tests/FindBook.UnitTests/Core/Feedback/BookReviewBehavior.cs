using FindBook.Core.Feedback.BookReviewAggregate;
using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.UnitTests.Core.Feedback;

public class BookReviewBehavior
{
  [Fact]
  public void ReviewCanBeUpdated()
  {
    var review = new BookReview(
      BookId.From(1),
      UserAccountId.From(2),
      StarRating.From(4),
      ReviewTitle.From("Great"),
      ReviewContent.From("Really enjoyed it."),
      DateTimeOffset.Parse("2026-03-25T10:00:00+05:30"));

    review.Update(
      StarRating.From(5),
      ReviewTitle.From("Excellent"),
      ReviewContent.From("Even better on a re-read."),
      DateTimeOffset.Parse("2026-03-26T10:00:00+05:30"));

    review.Rating.ShouldBe(StarRating.From(5));
    review.Title.ShouldBe(ReviewTitle.From("Excellent"));
    review.UpdatedOn.ShouldNotBeNull();
  }
}
