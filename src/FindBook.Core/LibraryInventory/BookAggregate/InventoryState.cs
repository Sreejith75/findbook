namespace FindBook.Core.LibraryInventory.BookAggregate;

public class InventoryState : ValueObject
{
  private InventoryState() { }

  public InventoryState(int totalCopies, int availableCopies)
  {
    if (totalCopies <= 0)
    {
      throw new InvalidOperationException("Total copies must be greater than zero.");
    }

    if (availableCopies < 0)
    {
      throw new InvalidOperationException("Available copies cannot be negative.");
    }

    if (availableCopies > totalCopies)
    {
      throw new InvalidOperationException("Available copies cannot exceed total copies.");
    }

    TotalCopies = totalCopies;
    AvailableCopies = availableCopies;
  }

  public int TotalCopies { get; private set; }
  public int AvailableCopies { get; private set; }
  public bool HasAvailableCopies => AvailableCopies > 0;

  public InventoryState ReserveCopy()
  {
    if (!HasAvailableCopies)
    {
      throw new InvalidOperationException("No copies are currently available.");
    }

    return new InventoryState(TotalCopies, AvailableCopies - 1);
  }

  public InventoryState ReturnCopy()
  {
    if (AvailableCopies >= TotalCopies)
    {
      throw new InvalidOperationException("Inventory is already fully stocked.");
    }

    return new InventoryState(TotalCopies, AvailableCopies + 1);
  }

  public InventoryState Adjust(int totalCopies, int availableCopies) => new(totalCopies, availableCopies);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return TotalCopies;
    yield return AvailableCopies;
  }
}
