using FindBook.Core.Delivery.DeliveryTaskAggregate;
using FindBook.Core.Delivery.DeliveryTaskAggregate.Specifications;
using FindBook.Core.Feedback.BookReviewAggregate;
using FindBook.Core.Feedback.BookReviewAggregate.Specifications;
using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.LibraryInventory.BookAggregate.Specifications;
using FindBook.Core.LibraryInventory.CategoryAggregate;
using FindBook.Core.LibraryInventory.CategoryAggregate.Specifications;
using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate.Specifications;
using FindBook.Core.Rental.RentalAggregate;
using FindBook.Core.Rental.RentalAggregate.Specifications;
using FindBook.Core.UserManagement.UserAccountAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate.Specifications;

namespace FindBook.UseCases.Admin;

public sealed record AdminOverviewDto(
  int TotalUsers,
  int TotalLibraries,
  int TotalCategories,
  int TotalBooks,
  int AvailableBooks,
  int ActiveRentals,
  int OverdueRentals,
  int OpenDeliveryTasks,
  int CompletedDeliveryTasks,
  int TotalReviews);

public sealed record GetAdminOverviewQuery() : IQuery<Result<AdminOverviewDto>>;

public sealed class GetAdminOverviewHandler(
  IReadRepository<UserAccount> userRepository,
  IReadRepository<Library> libraryRepository,
  IReadRepository<Category> categoryRepository,
  IReadRepository<Book> bookRepository,
  IReadRepository<Rental> rentalRepository,
  IReadRepository<DeliveryTask> deliveryRepository,
  IReadRepository<BookReview> reviewRepository)
  : IQueryHandler<GetAdminOverviewQuery, Result<AdminOverviewDto>>
{
  public async ValueTask<Result<AdminOverviewDto>> Handle(GetAdminOverviewQuery query, CancellationToken cancellationToken)
  {
    var totalUsers = await userRepository.CountAsync(new ListUserAccountsSpec(), cancellationToken);
    var totalLibraries = await libraryRepository.CountAsync(new ListLibrariesSpec(), cancellationToken);
    var totalCategories = await categoryRepository.CountAsync(new ListCategoriesSpec(), cancellationToken);
    var totalBooks = await bookRepository.CountAsync(new ListBooksSpec(), cancellationToken);
    var availableBooks = await bookRepository.CountAsync(new ListBooksSpec(availableOnly: true), cancellationToken);
    var activeRentals = await rentalRepository.CountAsync(new ListRentalsSpec(status: RentalStatus.Active), cancellationToken);
    var overdueRentals = await rentalRepository.CountAsync(new ListRentalsSpec(status: RentalStatus.Overdue), cancellationToken);
    var openTasks = await deliveryRepository.CountAsync(new ListDeliveryTasksSpec(status: DeliveryTaskStatus.Assigned), cancellationToken)
      + await deliveryRepository.CountAsync(new ListDeliveryTasksSpec(status: DeliveryTaskStatus.InTransit), cancellationToken);
    var completedTasks = await deliveryRepository.CountAsync(new ListDeliveryTasksSpec(status: DeliveryTaskStatus.Completed), cancellationToken);
    var totalReviews = await reviewRepository.CountAsync(new ListBookReviewsSpec(), cancellationToken);

    return Result.Success(new AdminOverviewDto(totalUsers, totalLibraries, totalCategories, totalBooks, availableBooks, activeRentals, overdueRentals, openTasks, completedTasks, totalReviews));
  }
}
