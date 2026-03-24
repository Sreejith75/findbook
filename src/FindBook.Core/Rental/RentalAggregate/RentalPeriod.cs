namespace FindBook.Core.Rental.RentalAggregate;

public class RentalPeriod : ValueObject
{
  private RentalPeriod() { }

  public RentalPeriod(DateOnly rentedOn, DateOnly dueOn)
  {
    if (dueOn < rentedOn)
    {
      throw new InvalidOperationException("Due date cannot be earlier than the rental date.");
    }

    RentedOn = rentedOn;
    DueOn = dueOn;
  }

  public static RentalPeriod Create(DateOnly rentedOn, int durationDays)
  {
    if (durationDays <= 0)
    {
      throw new InvalidOperationException("Rental duration must be greater than zero.");
    }

    return new RentalPeriod(rentedOn, rentedOn.AddDays(durationDays));
  }

  public DateOnly RentedOn { get; private set; }
  public DateOnly DueOn { get; private set; }

  public bool IsOverdue(DateOnly onDate) => onDate > DueOn;

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return RentedOn;
    yield return DueOn;
  }
}
