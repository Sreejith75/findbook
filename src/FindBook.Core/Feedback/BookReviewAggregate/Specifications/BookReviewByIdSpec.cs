namespace FindBook.Core.Feedback.BookReviewAggregate.Specifications;

public sealed class BookReviewByIdSpec : Specification<BookReview>, ISingleResultSpecification<BookReview>
{
  public BookReviewByIdSpec(BookReviewId id)
  {
    Query.Where(x => x.Id == id);
  }
}
