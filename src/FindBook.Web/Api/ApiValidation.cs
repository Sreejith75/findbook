using Ardalis.SmartEnum;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FindBook.Web.Api;

public sealed class ValidationErrorBuilder
{
  private readonly Dictionary<string, List<string>> _errors = new(StringComparer.OrdinalIgnoreCase);

  public bool HasErrors => _errors.Count > 0;

  public void Add(string key, string message)
  {
    if (!_errors.TryGetValue(key, out var values))
    {
      values = [];
      _errors[key] = values;
    }

    values.Add(message);
  }

  public Dictionary<string, string[]> ToDictionary() =>
    _errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Distinct().ToArray(), StringComparer.OrdinalIgnoreCase);
}

public static class ApiValidation
{
  public static bool TryCreate<T>(Func<T> factory, string key, ValidationErrorBuilder errors, out T value)
  {
    try
    {
      value = factory();
      return true;
    }
    catch (Exception ex)
    {
      errors.Add(key, ex.Message);
      value = default!;
      return false;
    }
  }

  public static bool TryParseSmartEnum<TEnum>(string value, string key, ValidationErrorBuilder errors, out TEnum enumValue)
    where TEnum : SmartEnum<TEnum>
  {
    try
    {
      enumValue = SmartEnum<TEnum>.FromName(value, true);
      return true;
    }
    catch
    {
      errors.Add(key, $"'{value}' is not a valid {typeof(TEnum).Name}.");
      enumValue = default!;
      return false;
    }
  }

  public static ValidationProblem ValidationProblem(ValidationErrorBuilder errors) =>
    TypedResults.ValidationProblem(errors.ToDictionary());
}
