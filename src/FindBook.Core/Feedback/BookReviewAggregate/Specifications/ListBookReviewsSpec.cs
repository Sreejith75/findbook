using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.Core.Feedback.BookReviewAggregate.Specifications;

public sealed class ListBookReviewsSpec : Specification<BookReview>
{
  public ListBookReviewsSpec(BookId? bookId = null, UserAccountId? reviewerAccountId = null)
  {
    if (bookId.HasValue)
    {
      Query.Where(x => x.BookId == bookId.Value);
    }

    if (reviewerAccountId.HasValue)
    {
      Query.Where(x => x.ReviewerAccountId == reviewerAccountId.Value);
    }

    Query.OrderByDescending(x => x.CreatedOn);
  }
}
