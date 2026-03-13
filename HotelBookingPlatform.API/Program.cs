using Serilog;
using AutoMapper;
using HotelBookingPlatform.API.Profiles;
using HotelBookingPlatform.Application.Services;
using HotelBookingPlatform.Infrastructure.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;






var builder = WebApplication.CreateBuilder(args);

// ✅ Prevent default inbound claim mapping issues (important for NameIdentifier/sub)
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;


// Configure Serilog
SerilogConfiguration.ConfigureLogger();
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(CityMappingProfile));

builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontend", policy => {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();      
    });
});

// Configure and register booking cleanup service
builder.Services.Configure<BookingCleanupOptions>(
    builder.Configuration.GetSection(BookingCleanupOptions.ConfigSection));
builder.Services.AddHostedService<BookingCleanupBackgroundService>();

// Add custom dependencies
builder.Services.AddApplicationDependencies()
                .AddPresentationDependencies(builder.Configuration)
                .AddInfrastructureDependencies()
                .AddSwaggerDocumentation()
                .AddCloudinary(builder.Configuration);

var app = builder.Build();

await IdentitySeeder.SeedAsync(app.Services, app.Configuration);


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HotelBooking API V1");
});

app.UseCors("AllowFrontend");
app.UseMiddleware<GlobalExceptionHandling>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();
