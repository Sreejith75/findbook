using FindBook.UseCases.Admin;
using FindBook.Web.Api;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace FindBook.Web.Admin;

public static class AdminEndpoints
{
  public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/admin").WithTags("Admin").RequireAuthorization();

    group.MapGet("/overview", async Task<HttpResult> (IMediator mediator, CancellationToken cancellationToken) =>
      (await mediator.Send(new GetAdminOverviewQuery(), cancellationToken)).ToHttpResult(item => TypedResults.Ok(new AdminOverviewResponse(item.TotalUsers, item.TotalLibraries, item.TotalCategories, item.TotalBooks, item.AvailableBooks, item.ActiveRentals, item.OverdueRentals, item.OpenDeliveryTasks, item.CompletedDeliveryTasks, item.TotalReviews))));

    return app;
  }
}

public sealed record AdminOverviewResponse(int TotalUsers, int TotalLibraries, int TotalCategories, int TotalBooks, int AvailableBooks, int ActiveRentals, int OverdueRentals, int OpenDeliveryTasks, int CompletedDeliveryTasks, int TotalReviews);
