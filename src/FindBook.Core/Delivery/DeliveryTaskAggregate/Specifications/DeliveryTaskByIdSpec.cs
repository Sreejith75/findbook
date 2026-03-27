namespace FindBook.Core.Delivery.DeliveryTaskAggregate.Specifications;

public sealed class DeliveryTaskByIdSpec : Specification<DeliveryTask>, ISingleResultSpecification<DeliveryTask>
{
  public DeliveryTaskByIdSpec(DeliveryTaskId id)
  {
    Query.Where(x => x.Id == id);
  }
}
