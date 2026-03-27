using Microsoft.AspNetCore.Http.HttpResults;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace FindBook.Web.Api;

public static class ArdalisResultMapper
{
  public static HttpResult ToHttpResult<T>(this Result<T> result, Func<T, HttpResult> onSuccess)
  {
    return result.Status switch
    {
      ResultStatus.Ok => onSuccess(result.Value),
      ResultStatus.NotFound => TypedResults.NotFound(),
      ResultStatus.Invalid => TypedResults.ValidationProblem(
        result.ValidationErrors
          .GroupBy(x => x.Identifier ?? string.Empty)
          .ToDictionary(x => x.Key, x => x.Select(v => v.ErrorMessage).ToArray())),
      ResultStatus.Conflict => TypedResults.Conflict(new { message = string.Join("; ", result.Errors) }),
      _ => TypedResults.BadRequest(new { message = string.Join("; ", result.Errors) }),
    };
  }

  public static HttpResult ToHttpResult(this Result result, Func<HttpResult>? onSuccess = null)
  {
    return result.Status switch
    {
      ResultStatus.Ok => onSuccess?.Invoke() ?? TypedResults.Ok(),
      ResultStatus.NotFound => TypedResults.NotFound(),
      ResultStatus.Invalid => TypedResults.ValidationProblem(
        result.ValidationErrors
          .GroupBy(x => x.Identifier ?? string.Empty)
          .ToDictionary(x => x.Key, x => x.Select(v => v.ErrorMessage).ToArray())),
      ResultStatus.Conflict => TypedResults.Conflict(new { message = string.Join("; ", result.Errors) }),
      _ => TypedResults.BadRequest(new { message = string.Join("; ", result.Errors) }),
    };
  }
}
