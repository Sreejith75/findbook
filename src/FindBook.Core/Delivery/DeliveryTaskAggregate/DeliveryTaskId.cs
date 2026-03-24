using Vogen;

namespace FindBook.Core.Delivery.DeliveryTaskAggregate;

[ValueObject<int>]
public readonly partial struct DeliveryTaskId
{
  private static Validation Validate(int value) =>
    value > 0 ? Validation.Ok : Validation.Invalid("Delivery task ID must be positive.");
}
