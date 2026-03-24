namespace FindBook.Core.SharedKernel;

public class PostalAddress : ValueObject
{
  private PostalAddress() { }

  public PostalAddress(string street, string city, string state, string postalCode, string country)
  {
    Street = Guard.Against.NullOrWhiteSpace(street);
    City = Guard.Against.NullOrWhiteSpace(city);
    State = Guard.Against.NullOrWhiteSpace(state);
    PostalCode = Guard.Against.NullOrWhiteSpace(postalCode);
    Country = Guard.Against.NullOrWhiteSpace(country);
  }

  public string Street { get; private set; } = string.Empty;
  public string City { get; private set; } = string.Empty;
  public string State { get; private set; } = string.Empty;
  public string PostalCode { get; private set; } = string.Empty;
  public string Country { get; private set; } = string.Empty;

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Street;
    yield return City;
    yield return State;
    yield return PostalCode;
    yield return Country;
  }
}
