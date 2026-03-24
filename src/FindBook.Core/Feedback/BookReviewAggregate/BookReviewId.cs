using Vogen;

namespace FindBook.Core.Feedback.BookReviewAggregate;

[ValueObject<int>]
public readonly partial struct BookReviewId
{
  private static Validation Validate(int value) =>
    value > 0 ? Validation.Ok : Validation.Invalid("Book review ID must be positive.");
}
