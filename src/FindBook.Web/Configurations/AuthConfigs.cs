using FindBook.Infrastructure.Auth;
using FindBook.Web.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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

    services.AddScoped<ICurrentAppUserAccessor, CurrentAppUserAccessor>();
    services.AddTransient<IClaimsTransformation, LocalUserClaimsTransformation>();
    services.AddScoped<IAuthorizationHandler, FindBookAccessHandler>();

    services.AddAuthorization(options =>
    {
      options.AddPolicy(FindBookPolicies.Authenticated, policy => policy.AddRequirements(new FindBookAccessRequirement(FindBookAccessLevel.Authenticated)));
      options.AddPolicy(FindBookPolicies.UserOrHigher, policy => policy.AddRequirements(new FindBookAccessRequirement(FindBookAccessLevel.UserOrHigher)));
      options.AddPolicy(FindBookPolicies.DeliveryPartnerOrHigher, policy => policy.AddRequirements(new FindBookAccessRequirement(FindBookAccessLevel.DeliveryPartnerOrHigher)));
      options.AddPolicy(FindBookPolicies.Admin, policy => policy.AddRequirements(new FindBookAccessRequirement(FindBookAccessLevel.Admin)));
      options.AddPolicy(FindBookPolicies.SuperAdmin, policy => policy.AddRequirements(new FindBookAccessRequirement(FindBookAccessLevel.SuperAdmin)));
    });

    return services;
  }
}
