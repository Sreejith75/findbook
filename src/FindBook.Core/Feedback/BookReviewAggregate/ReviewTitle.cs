using Vogen;

namespace FindBook.Core.Feedback.BookReviewAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct ReviewTitle
{
  public const int MaxLength = 200;

  private static Validation Validate(in string value) =>
    string.IsNullOrWhiteSpace(value)
      ? Validation.Invalid("Review title is required.")
      : value.Length > MaxLength
        ? Validation.Invalid($"Review title cannot exceed {MaxLength} characters.")
        : Validation.Ok;
}
