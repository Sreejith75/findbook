namespace FindBook.Core.Delivery.DeliveryTaskAggregate;

public class DeliveryTaskStatus : SmartEnum<DeliveryTaskStatus>
{
  public static readonly DeliveryTaskStatus Assigned = new(nameof(Assigned), 1);
  public static readonly DeliveryTaskStatus InTransit = new(nameof(InTransit), 2);
  public static readonly DeliveryTaskStatus Completed = new(nameof(Completed), 3);
  public static readonly DeliveryTaskStatus Failed = new(nameof(Failed), 4);

  private DeliveryTaskStatus(string name, int value) : base(name, value) { }
}
