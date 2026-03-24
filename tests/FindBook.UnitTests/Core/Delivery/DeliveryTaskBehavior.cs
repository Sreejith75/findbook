using FindBook.Core.Delivery.DeliveryTaskAggregate;
using FindBook.Core.Rental.RentalAggregate;
using FindBook.Core.SharedKernel;
using FindBook.Core.UserManagement.UserAccountAggregate;

namespace FindBook.UnitTests.Core.Delivery;

public class DeliveryTaskBehavior
{
  [Fact]
  public void AssignedTaskCanMoveInTransitAndComplete()
  {
    var task = new DeliveryTask(
      RentalId.From(10),
      UserAccountId.From(11),
      DeliveryTaskType.Delivery,
      new DeliveryAddressSnapshot("Street", "City", "State", "12345", "India"),
      DateTimeOffset.Parse("2026-03-25T10:00:00+05:30"));

    task.MarkInTransit();
    task.Complete(DateTimeOffset.Parse("2026-03-25T12:00:00+05:30"));

    task.Status.ShouldBe(DeliveryTaskStatus.Completed);
    task.CompletedAt.ShouldNotBeNull();
  }

  [Fact]
  public void CompletedTaskCannotFail()
  {
    var task = new DeliveryTask(
      RentalId.From(10),
      UserAccountId.From(11),
      DeliveryTaskType.Pickup,
      new DeliveryAddressSnapshot("Street", "City", "State", "12345", "India"),
      DateTimeOffset.Parse("2026-03-25T10:00:00+05:30"));
    task.Complete(DateTimeOffset.Parse("2026-03-25T12:00:00+05:30"));

    Should.Throw<InvalidOperationException>(() => task.Fail());
  }
}
