namespace FindBook.Web.Api;

public sealed record AddressContract(
  string Street,
  string City,
  string State,
  string PostalCode,
  string Country,
  bool IsDefault = false);

public sealed record AddressPayload(
  string Street,
  string City,
  string State,
  string PostalCode,
  string Country);

public sealed record AddressView(
  string Street,
  string City,
  string State,
  string PostalCode,
  string Country);
