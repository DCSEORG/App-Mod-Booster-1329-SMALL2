using ExpenseApp.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// Services
// -----------------------------------------------------------------------
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Database service – reads connection string from configuration
builder.Services.AddSingleton<IDatabaseService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<DatabaseService>>();
    string connStr = config.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException("DefaultConnection not configured.");
    return new DatabaseService(connStr, logger);
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Expense Management API",
        Version     = "v1",
        Description = "REST API for the Expense Management System. All operations are backed by stored procedures."
    });
});

var app = builder.Build();

// -----------------------------------------------------------------------
// Middleware pipeline
// -----------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Swagger UI available at /swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Expense Management API v1");
    c.RoutePrefix = "swagger";
});

app.MapRazorPages();
app.MapControllers();

app.Run();
