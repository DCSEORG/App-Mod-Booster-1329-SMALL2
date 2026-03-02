using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApp.Controllers;

/// <summary>
/// REST API for Expense Statuses.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StatusesController : ControllerBase
{
    private readonly IDatabaseService _db;

    public StatusesController(IDatabaseService db) => _db = db;

    /// <summary>Get all expense status values.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ExpenseStatus>), 200)]
    public async Task<IActionResult> GetAll()
    {
        try   { return Ok(await _db.GetExpenseStatusesAsync()); }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
    }
}
