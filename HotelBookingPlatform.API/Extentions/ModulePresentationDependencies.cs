using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingPlatform.API.Extentions;

public static class ModulePresentationDependencies
{
    public static IServiceCollection AddPresentationDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<LocalUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true; 
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();

        // Configure JWT Authentication
        var jwtSettings = new JWT();
        configuration.GetSection("JWT").Bind(jwtSettings);

        services.Configure<JWT>(configuration.GetSection("JWT"));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true; 
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key!)), // ✅ Ensures Key isn't null
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero 
                };
            });

        // Authorization Policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
        });

        // Response Caching
        services.AddControllers(options =>
        {
            options.CacheProfiles.Add("DefaultCache", new CacheProfile
            {
                Duration = 30,
                Location = ResponseCacheLocation.Client 
            });
        });

        services.AddScoped<IResponseHandler, ResponseHandler>();
        services.AddScoped<ILog, Log>();

        // Configure Email Service
        var emailSettings = new EmailSettings();
        configuration.GetSection("EmailSettings").Bind(emailSettings);

        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}

