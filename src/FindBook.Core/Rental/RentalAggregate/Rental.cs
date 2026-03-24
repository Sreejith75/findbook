using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.SharedKernel;
using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.Core.Rental.RentalAggregate;

public class Rental : EntityBase<Rental, RentalId>, IAggregateRoot
{
  public const int DefaultRentalDurationDays = 14;

  private Rental() { }

  private Rental(
    UserAccountId userAccountId,
    BookId bookId,
    LibraryId libraryId,
    DeliveryAddressSnapshot deliveryAddress,
    RentalPeriod rentalPeriod)
  {
    UserAccountId = userAccountId;
    BookId = bookId;
    LibraryId = libraryId;
    DeliveryAddress = deliveryAddress;
    RentalPeriod = rentalPeriod;
    Status = RentalStatus.PendingDelivery;
  }

  public UserAccountId UserAccountId { get; private set; }
  public BookId BookId { get; private set; }
  public LibraryId LibraryId { get; private set; }
  public DeliveryAddressSnapshot DeliveryAddress { get; private set; } = null!;
  public RentalPeriod RentalPeriod { get; private set; } = null!;
  public RentalStatus Status { get; private set; } = RentalStatus.PendingDelivery;
  public DateOnly? ReturnRequestedOn { get; private set; }
  public DateOnly? ReturnedOn { get; private set; }

  public static Rental Create(
    UserAccountId userAccountId,
    BookId bookId,
    LibraryId libraryId,
    DeliveryAddressSnapshot deliveryAddress,
    DateOnly rentedOn) =>
    new(userAccountId, bookId, libraryId, deliveryAddress, RentalPeriod.Create(rentedOn, DefaultRentalDurationDays));

  public void MarkDelivered()
  {
    if (Status != RentalStatus.PendingDelivery)
    {
      throw new InvalidOperationException("Only pending rentals can be marked as delivered.");
    }

    Status = RentalStatus.Active;
  }

  public void RequestReturn(DateOnly requestedOn)
  {
    if (Status != RentalStatus.Active && Status != RentalStatus.Overdue)
    {
      throw new InvalidOperationException("Only active or overdue rentals can request a return.");
    }

    ReturnRequestedOn = requestedOn;
    Status = RentalStatus.ReturnRequested;
  }

  public void CompleteReturn(DateOnly returnedOn)
  {
    if (Status == RentalStatus.PendingDelivery)
    {
      throw new InvalidOperationException("A rental cannot be returned before it is delivered.");
    }

    if (Status == RentalStatus.Returned)
    {
      throw new InvalidOperationException("Rental has already been returned.");
    }

    ReturnedOn = returnedOn;
    Status = RentalStatus.Returned;
  }

  public void MarkOverdueIfNeeded(DateOnly today)
  {
    if (Status == RentalStatus.Active && RentalPeriod.IsOverdue(today))
    {
      Status = RentalStatus.Overdue;
    }
  }
}
