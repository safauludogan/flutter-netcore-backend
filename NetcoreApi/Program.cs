using Microsoft.EntityFrameworkCore;
using NetcoreApi;
using NetcoreApi.Middleware;
using NetcoreApi.Services.Abstract;
using NetcoreApi.Services.Concrete;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Project Environments
var env = builder.Environment;

builder.Configuration
    .SetBasePath(env.ContentRootPath)
     .AddJsonFile("appsettings.json", optional: false)
     .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
#endregion

// Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Configure Swagger to work in production
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NetcoreApi V1");
    c.RoutePrefix = "swagger";
});

if (!app.Environment.IsDevelopment())
{
    // Production error handler
    app.UseExceptionHandler("/error");
}

app.UseCors("AllowAll");

// Custom Middleware
app.UseMiddleware<ErrorSimulatorMiddleware>();
app.UseMiddleware<DelayMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = "/uploads"
});
app.UseAuthorization();
app.MapControllers();

// Custom Middleware
app.UseMiddleware<ErrorSimulatorMiddleware>();
app.UseMiddleware<DelayMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles(); // For file uploads/downloads
app.UseAuthorization();
app.MapControllers();

app.Run();