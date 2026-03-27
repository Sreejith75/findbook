namespace FindBook.Core.Rental.RentalAggregate.Specifications;

public sealed class RentalByIdSpec : Specification<Rental>, ISingleResultSpecification<Rental>
{
  public RentalByIdSpec(RentalId id)
  {
    Query.Where(x => x.Id == id);
  }
}
