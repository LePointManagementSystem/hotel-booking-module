using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
SerilogConfiguration.ConfigureLogger();
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseCors();
app.UseMiddleware<GlobalExceptionHandling>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
