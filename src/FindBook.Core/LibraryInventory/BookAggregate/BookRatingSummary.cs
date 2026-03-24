namespace FindBook.Core.LibraryInventory.BookAggregate;

public class BookRatingSummary : ValueObject
{
  private BookRatingSummary() { }

  public BookRatingSummary(decimal averageRating, int ratingCount)
  {
    if (averageRating < 0m || averageRating > 5m)
    {
      throw new InvalidOperationException("Average rating must be between 0 and 5.");
    }

    if (ratingCount < 0)
    {
      throw new InvalidOperationException("Rating count cannot be negative.");
    }

    AverageRating = averageRating;
    RatingCount = ratingCount;
  }

  public static BookRatingSummary Empty => new(0m, 0);

  public decimal AverageRating { get; private set; }
  public int RatingCount { get; private set; }

  public BookRatingSummary AddRating(int rating)
  {
    if (rating < 1 || rating > 5)
    {
      throw new InvalidOperationException("Rating must be between 1 and 5.");
    }

    var newCount = RatingCount + 1;
    var newAverage = ((AverageRating * RatingCount) + rating) / newCount;
    return new BookRatingSummary(decimal.Round(newAverage, 2, MidpointRounding.AwayFromZero), newCount);
  }

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return AverageRating;
    yield return RatingCount;
  }
}
