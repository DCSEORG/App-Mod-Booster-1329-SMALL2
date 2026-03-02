using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApp.Controllers;

/// <summary>
/// REST API for Expense Categories.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly IDatabaseService _db;

    public CategoriesController(IDatabaseService db) => _db = db;

    /// <summary>Get all active expense categories.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ExpenseCategory>), 200)]
    public async Task<IActionResult> GetAll()
    {
        try   { return Ok(await _db.GetExpenseCategoriesAsync()); }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
    }
}
