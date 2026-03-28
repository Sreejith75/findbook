using System.Text.Json;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace FindBook.Infrastructure.Auth;

public sealed class FirebaseAdminStartup
{
  private readonly FirebaseAuthOptions _options;
  private readonly ILogger<FirebaseAdminStartup> _logger;
  private readonly string _contentRootPath;

  public FirebaseAdminStartup(
    IOptions<FirebaseAuthOptions> options,
    ILogger<FirebaseAdminStartup> logger,
    string contentRootPath)
  {
    _options = options.Value;
    _logger = logger;
    _contentRootPath = contentRootPath;
  }

  public FirebaseApp Initialize()
  {
    var resolvedPath = ResolveServiceAccountPath(_options.ServiceAccountPath, _contentRootPath);

    if (!File.Exists(resolvedPath))
    {
      throw new FileNotFoundException(
        $"Firebase service account file was not found at '{resolvedPath}'.",
        resolvedPath);
    }

    var projectId = ResolveProjectId(resolvedPath, _options.ProjectId);

    if (FirebaseApp.DefaultInstance is not null)
    {
      return FirebaseApp.DefaultInstance;
    }

    var app = FirebaseApp.Create(new AppOptions
    {
      Credential = CredentialFactory
        .FromFile<ServiceAccountCredential>(resolvedPath)
        .ToGoogleCredential(),
      ProjectId = projectId
    });

    _logger.LogInformation("Firebase Admin initialized for project {ProjectId}", projectId);

    return app;
  }

  public static string ResolveServiceAccountPath(string configuredPath, string contentRootPath)
  {
    if (Path.IsPathRooted(configuredPath))
    {
      return configuredPath;
    }

    return Path.GetFullPath(Path.Combine(contentRootPath, configuredPath));
  }

  public static string ResolveProjectId(string serviceAccountPath, string? configuredProjectId)
  {
    if (!string.IsNullOrWhiteSpace(configuredProjectId))
    {
      return configuredProjectId;
    }

    using var stream = File.OpenRead(serviceAccountPath);
    using var document = JsonDocument.Parse(stream);

    if (document.RootElement.TryGetProperty("project_id", out var projectIdProperty) &&
        !string.IsNullOrWhiteSpace(projectIdProperty.GetString()))
    {
      return projectIdProperty.GetString()!;
    }

    throw new InvalidOperationException(
      "Firebase project id is missing. Set Firebase:ProjectId or include project_id in the service account file.");
  }
}
