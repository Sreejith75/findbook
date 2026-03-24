using FindBook.Core.Rental.RentalAggregate;
using FindBook.Core.SharedKernel;
using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.Core.Delivery.DeliveryTaskAggregate;

public class DeliveryTask : EntityBase<DeliveryTask, DeliveryTaskId>, IAggregateRoot
{
  private DeliveryTask() { }

  public DeliveryTask(
    RentalId rentalId,
    UserAccountId deliveryPartnerAccountId,
    DeliveryTaskType type,
    DeliveryAddressSnapshot deliveryAddress,
    DateTimeOffset assignedAt)
  {
    RentalId = rentalId;
    DeliveryPartnerAccountId = deliveryPartnerAccountId;
    Type = type;
    DeliveryAddress = deliveryAddress;
    AssignedAt = assignedAt;
    Status = DeliveryTaskStatus.Assigned;
  }

  public RentalId RentalId { get; private set; }
  public UserAccountId DeliveryPartnerAccountId { get; private set; }
  public DeliveryTaskType Type { get; private set; } = DeliveryTaskType.Delivery;
  public DeliveryTaskStatus Status { get; private set; } = DeliveryTaskStatus.Assigned;
  public DeliveryAddressSnapshot DeliveryAddress { get; private set; } = null!;
  public DateTimeOffset AssignedAt { get; private set; }
  public DateTimeOffset? CompletedAt { get; private set; }

  public void MarkInTransit()
  {
    if (Status != DeliveryTaskStatus.Assigned)
    {
      throw new InvalidOperationException("Only assigned tasks can move in transit.");
    }

    Status = DeliveryTaskStatus.InTransit;
  }

  public void Complete(DateTimeOffset completedAt)
  {
    if (Status != DeliveryTaskStatus.Assigned && Status != DeliveryTaskStatus.InTransit)
    {
      throw new InvalidOperationException("Only active delivery tasks can be completed.");
    }

    Status = DeliveryTaskStatus.Completed;
    CompletedAt = completedAt;
  }

  public void Fail()
  {
    if (Status == DeliveryTaskStatus.Completed)
    {
      throw new InvalidOperationException("A completed task cannot be failed.");
    }

    Status = DeliveryTaskStatus.Failed;
  }
}
