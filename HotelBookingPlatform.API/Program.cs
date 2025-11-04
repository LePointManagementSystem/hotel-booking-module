using Serilog;
using AutoMapper;
using HotelBookingPlatform.API.Profiles;
using HotelBookingPlatform.Application.Services;




var builder = WebApplication.CreateBuilder(args);

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

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseMiddleware<GlobalExceptionHandling>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();
