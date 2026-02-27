using ExpensesApp.Data;
using ExpensesApp.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
// ReSharper disable once RedundantUsingDirective

namespace ExpensesApp.Controllers;

/// <summary>
/// Lookup data API – Roles, Categories and Statuses.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
public class LookupsController : ControllerBase
{
    private readonly ExpenseRepository _repo;
    public LookupsController(ExpenseRepository repo) => _repo = repo;

    // ── GET /api/roles ────────────────────────────────────────────────────────
    [HttpGet("roles")]
    [SwaggerOperation(Summary = "Get all roles", Description = "Uses dbo.GetAllRoles.")]
    [SwaggerResponse(200, "List of roles", typeof(List<Role>))]
    public async Task<IActionResult> GetRoles()
    {
        var (roles, _) = await _repo.GetAllRolesAsync();
        return Ok(roles);
    }

    // ── GET /api/categories ───────────────────────────────────────────────────
    [HttpGet("categories")]
    [SwaggerOperation(Summary = "Get all expense categories", Description = "Uses dbo.GetAllExpenseCategories.")]
    [SwaggerResponse(200, "List of categories", typeof(List<ExpenseCategory>))]
    public async Task<IActionResult> GetCategories()
    {
        var (cats, _) = await _repo.GetAllCategoriesAsync();
        return Ok(cats);
    }

    // ── POST /api/categories ──────────────────────────────────────────────────
    [HttpPost("categories")]
    [SwaggerOperation(Summary = "Create expense category", Description = "Uses dbo.CreateExpenseCategory.")]
    [SwaggerResponse(201, "Category created")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.CategoryName))
            return BadRequest(new { message = "CategoryName is required." });
        var (newId, err) = await _repo.CreateCategoryAsync(req.CategoryName);
        if (err != null || newId == 0)
            return StatusCode(503, new { message = err ?? "Failed." });
        return Created($"/api/categories/{newId}", new { categoryId = newId });
    }

    // ── GET /api/statuses ─────────────────────────────────────────────────────
    [HttpGet("statuses")]
    [SwaggerOperation(Summary = "Get all expense statuses", Description = "Uses dbo.GetAllExpenseStatuses.")]
    [SwaggerResponse(200, "List of statuses", typeof(List<ExpenseStatus>))]
    public async Task<IActionResult> GetStatuses()
    {
        var (statuses, _) = await _repo.GetAllStatusesAsync();
        return Ok(statuses);
    }
}

public class CreateCategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
}
