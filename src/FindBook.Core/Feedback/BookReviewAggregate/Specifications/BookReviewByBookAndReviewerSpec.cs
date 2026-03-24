using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.Core.Feedback.BookReviewAggregate.Specifications;

public class BookReviewByBookAndReviewerSpec : Specification<BookReview>
{
  public BookReviewByBookAndReviewerSpec(BookId bookId, UserAccountId reviewerAccountId)
  {
    Query.Where(x => x.BookId == bookId && x.ReviewerAccountId == reviewerAccountId);
  }
}
