namespace FindBook.Core.Rental.RentalAggregate;

public class RentalStatus : SmartEnum<RentalStatus>
{
  public static readonly RentalStatus PendingDelivery = new(nameof(PendingDelivery), 1);
  public static readonly RentalStatus Active = new(nameof(Active), 2);
  public static readonly RentalStatus ReturnRequested = new(nameof(ReturnRequested), 3);
  public static readonly RentalStatus Returned = new(nameof(Returned), 4);
  public static readonly RentalStatus Overdue = new(nameof(Overdue), 5);

  private RentalStatus(string name, int value) : base(name, value) { }
}
