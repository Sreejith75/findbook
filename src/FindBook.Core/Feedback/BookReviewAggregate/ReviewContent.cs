using Vogen;

namespace FindBook.Core.Feedback.BookReviewAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct ReviewContent
{
  public const int MaxLength = 2000;

  private static Validation Validate(in string value) =>
    string.IsNullOrWhiteSpace(value)
      ? Validation.Invalid("Review content is required.")
      : value.Length > MaxLength
        ? Validation.Invalid($"Review content cannot exceed {MaxLength} characters.")
        : Validation.Ok;
}
