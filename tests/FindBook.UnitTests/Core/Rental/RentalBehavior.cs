using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.Rental.RentalAggregate;
using FindBook.Core.SharedKernel;
using FindBook.Core.UserManagement.UserAccountAggregate;
using RentalAggregate = FindBook.Core.Rental.RentalAggregate.Rental;

namespace FindBook.UnitTests.Core.Rentals;

public class RentalBehavior
{
  [Fact]
  public void NewRentalStartsPendingDeliveryWithDefaultDueDate()
  {
    var rentedOn = new DateOnly(2026, 3, 25);

    var rental = RentalAggregate.Create(
      UserAccountId.From(1),
      BookId.From(2),
      LibraryId.From(3),
      new DeliveryAddressSnapshot("Street", "City", "State", "12345", "India"),
      rentedOn);

    rental.Status.ShouldBe(RentalStatus.PendingDelivery);
    rental.RentalPeriod.DueOn.ShouldBe(rentedOn.AddDays(RentalAggregate.DefaultRentalDurationDays));
  }

  [Fact]
  public void RentalMovesThroughDeliveryAndReturnFlow()
  {
    var rental = CreateRental();

    rental.MarkDelivered();
    rental.RequestReturn(new DateOnly(2026, 4, 1));
    rental.CompleteReturn(new DateOnly(2026, 4, 2));

    rental.Status.ShouldBe(RentalStatus.Returned);
    rental.ReturnedOn.ShouldBe(new DateOnly(2026, 4, 2));
  }

  [Fact]
  public void ActiveRentalCanBecomeOverdue()
  {
    var rental = CreateRental();
    rental.MarkDelivered();

    rental.MarkOverdueIfNeeded(new DateOnly(2026, 4, 20));

    rental.Status.ShouldBe(RentalStatus.Overdue);
  }

  private static RentalAggregate CreateRental() =>
    RentalAggregate.Create(
      UserAccountId.From(1),
      BookId.From(2),
      LibraryId.From(3),
      new DeliveryAddressSnapshot("Street", "City", "State", "12345", "India"),
      new DateOnly(2026, 3, 25));
}
