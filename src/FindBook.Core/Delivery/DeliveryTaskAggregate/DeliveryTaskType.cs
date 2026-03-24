namespace FindBook.Core.Delivery.DeliveryTaskAggregate;

public class DeliveryTaskType : SmartEnum<DeliveryTaskType>
{
  public static readonly DeliveryTaskType Delivery = new(nameof(Delivery), 1);
  public static readonly DeliveryTaskType Pickup = new(nameof(Pickup), 2);

  private DeliveryTaskType(string name, int value) : base(name, value) { }
}
