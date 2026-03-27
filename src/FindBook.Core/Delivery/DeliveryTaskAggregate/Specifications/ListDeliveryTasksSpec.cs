using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.Core.Delivery.DeliveryTaskAggregate.Specifications;

public sealed class ListDeliveryTasksSpec : Specification<DeliveryTask>
{
  public ListDeliveryTasksSpec(UserAccountId? deliveryPartnerAccountId = null, DeliveryTaskStatus? status = null, DeliveryTaskType? type = null)
  {
    if (deliveryPartnerAccountId.HasValue)
    {
      Query.Where(x => x.DeliveryPartnerAccountId == deliveryPartnerAccountId.Value);
    }

    if (status is not null)
    {
      Query.Where(x => x.Status == status);
    }

    if (type is not null)
    {
      Query.Where(x => x.Type == type);
    }

    Query.OrderByDescending(x => x.AssignedAt);
  }
}
