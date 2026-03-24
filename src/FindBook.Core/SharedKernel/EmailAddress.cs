using System.Net.Mail;
using Vogen;

namespace FindBook.Core.SharedKernel;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct EmailAddress
{
  private static Validation Validate(in string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      return Validation.Invalid("Email address is required.");
    }

    try
    {
      _ = new MailAddress(value);
      return Validation.Ok;
    }
    catch
    {
      return Validation.Invalid("Email address is invalid.");
    }
  }
}
