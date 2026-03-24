using Vogen;

namespace FindBook.Core.Rental.RentalAggregate;

[ValueObject<int>]
public readonly partial struct RentalId
{
  private static Validation Validate(int value) =>
    value > 0 ? Validation.Ok : Validation.Invalid("Rental ID must be positive.");
}
