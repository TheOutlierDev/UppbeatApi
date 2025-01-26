using UppbeatApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add custom service configurations
builder.Services.AddSwaggerConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddRateLimiting(builder.Configuration);
builder.Services.AddScopedServices();
builder.Services.AddSingletonServices();

// Add authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("DOCKER_ENVIRONMENT") == "true")
{
    app.UseSwaggerConfiguration();
}

// Use rate limiting
app.UseRateLimiting();

app.UseHttpsRedirection();

// Enable authentication and authorization in the correct order
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
