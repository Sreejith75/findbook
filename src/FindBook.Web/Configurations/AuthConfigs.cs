using FindBook.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FindBook.Web.Configurations;

public static class AuthConfigs
{
  public static IServiceCollection AddFirebaseAuth(
    this IServiceCollection services,
    IConfiguration configuration,
    string contentRootPath)
  {
    var options = configuration.GetSection(FirebaseAuthOptions.SectionName).Get<FirebaseAuthOptions>()
      ?? new FirebaseAuthOptions();

    var resolvedPath = FirebaseAdminStartup.ResolveServiceAccountPath(options.ServiceAccountPath, contentRootPath);
    var projectId = FirebaseAdminStartup.ResolveProjectId(resolvedPath, options.ProjectId);
    var issuer = $"https://securetoken.google.com/{projectId}";

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        options.Authority = issuer;
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ClockSkew = TimeSpan.FromMinutes(2),
          NameClaimType = "name",
          RoleClaimType = "role",
          ValidAudience = projectId,
          ValidIssuer = issuer,
          ValidateAudience = true,
          ValidateIssuer = true,
          ValidateIssuerSigningKey = true,
          ValidateLifetime = true
        };
      });

    services.AddAuthorization();

    return services;
  }
}
