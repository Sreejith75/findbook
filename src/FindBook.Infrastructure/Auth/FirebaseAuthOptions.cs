namespace FindBook.Infrastructure.Auth;

public sealed class FirebaseAuthOptions
{
  public const string SectionName = "Firebase";

  public string? ProjectId { get; set; }

  public string ServiceAccountPath { get; set; } = "../FindBook.Infrastructure/Auth/firebase-key.json";
}
