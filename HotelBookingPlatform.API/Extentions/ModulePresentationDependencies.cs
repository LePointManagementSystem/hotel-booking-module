using HotelBookingPlatform.Application.Services;
using HotelBookingPlatform.Domain.Helpers;
using HotelBookingPlatform.Domain.IServices;
using HotelBookingPlatform.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

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

        // JWT settings
        var jwtSettings = new JWT();
        configuration.GetSection("JWT").Bind(jwtSettings);
        services.Configure<JWT>(configuration.GetSection("JWT"));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;

                // IMPORTANT: si tu as DefaultMapInboundClaims = false dans Program.cs, c'est cohérent
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key!)),

                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    // ✅ FIX: pour que User.IsInRole("Admin") marche
                    RoleClaimType = ClaimTypes.Role,

                    // ✅ FIX: pour que ClaimTypes.NameIdentifier soit bien utilisé
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
        });

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

        // Email settings
        var emailSettings = new EmailSettings();
        configuration.GetSection("EmailSettings").Bind(emailSettings);

        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
