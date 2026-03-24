using Vogen;

namespace FindBook.Core.Feedback.BookReviewAggregate;

[ValueObject<int>(conversions: Conversions.SystemTextJson)]
public readonly partial struct StarRating
{
  private static Validation Validate(int value) =>
    value >= 1 && value <= 5
      ? Validation.Ok
      : Validation.Invalid("Star rating must be between 1 and 5.");
}
