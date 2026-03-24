using System.Text.RegularExpressions;
using Vogen;

namespace FindBook.Core.LibraryInventory.BookAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct Isbn
{
  private static readonly Regex IsbnPattern = new("^[0-9Xx-]{10,17}$", RegexOptions.Compiled);

  private static Validation Validate(in string value) =>
    string.IsNullOrWhiteSpace(value)
      ? Validation.Invalid("ISBN is required.")
      : IsbnPattern.IsMatch(value)
        ? Validation.Ok
        : Validation.Invalid("ISBN format is invalid.");
}
