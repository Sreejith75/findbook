using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.Core.Rental.RentalAggregate.Specifications;

public sealed class ListRentalsSpec : Specification<Rental>
{
  public ListRentalsSpec(UserAccountId? userAccountId = null, BookId? bookId = null, RentalStatus? status = null)
  {
    if (userAccountId.HasValue)
    {
      Query.Where(x => x.UserAccountId == userAccountId.Value);
    }

    if (bookId.HasValue)
    {
      Query.Where(x => x.BookId == bookId.Value);
    }

    if (status is not null)
    {
      Query.Where(x => x.Status == status);
    }

    Query.OrderByDescending(x => x.RentalPeriod.RentedOn);
  }
}
