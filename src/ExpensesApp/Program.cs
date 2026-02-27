using ExpensesApp.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Connection string ─────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
    ?? "";

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Register repository as singleton (connection string doesn't change at runtime)
builder.Services.AddSingleton<ExpenseRepository>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<ExpenseRepository>>();
    return new ExpenseRepository(connectionString, logger);
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Expenses Management API",
        Version     = "v1",
        Description = "RESTful API for the Expenses Management System. " +
                      "All operations use stored procedures exclusively.",
        Contact     = new OpenApiContact { Name = "Expenses System" }
    });
    c.EnableAnnotations();
});

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (!app.Environment.IsProduction())
    app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Swagger – available in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Expenses Management API v1");
    c.RoutePrefix = "swagger";
});

app.MapRazorPages();
app.MapControllers();

app.Run();
