using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApp.Controllers;

/// <summary>
/// REST API for Expenses.  All database operations use stored procedures.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExpensesController : ControllerBase
{
    private readonly IDatabaseService _db;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(IDatabaseService db, ILogger<ExpensesController> logger)
    {
        _db     = db;
        _logger = logger;
    }

    /// <summary>Get all expenses.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<Expense>), 200)]
    public async Task<IActionResult> GetAll()
    {
        try   { return Ok(await _db.GetExpensesAsync()); }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
    }

    /// <summary>Get a single expense by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Expense), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var expense = await _db.GetExpenseByIdAsync(id);
            return expense is null ? NotFound($"Expense {id} not found.") : Ok(expense);
        }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
    }

    /// <summary>Get all expenses for a specific user.</summary>
    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(typeof(List<Expense>), 200)]
    public async Task<IActionResult> GetByUser(int userId)
    {
        try   { return Ok(await _db.GetExpensesByUserAsync(userId)); }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
    }

    /// <summary>Get all expenses with a given status (Draft, Submitted, Approved, Rejected).</summary>
    [HttpGet("status/{statusName}")]
    [ProducesResponseType(typeof(List<Expense>), 200)]
    public async Task<IActionResult> GetByStatus(string statusName)
    {
        try   { return Ok(await _db.GetExpensesByStatusAsync(statusName)); }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
    }

    /// <summary>Create a new expense.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateExpenseRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();
        try
        {
            int newId = await _db.CreateExpenseAsync(req);
            return CreatedAtAction(nameof(GetById), new { id = newId }, new { expenseId = newId });
        }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
    }

    /// <summary>Update a draft expense.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();
        try
        {
            int rows = await _db.UpdateExpenseAsync(id, req);
            return rows == 0 ? NotFound($"Expense {id} not found or is not in Draft status.") : Ok(new { rowsAffected = rows });
        }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
    }

    /// <summary>Delete a draft expense.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            int rows = await _db.DeleteExpenseAsync(id);
            return rows == 0 ? NotFound($"Expense {id} not found or is not in Draft status.") : Ok(new { rowsAffected = rows });
        }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
    }

    /// <summary>Approve a submitted expense.</summary>
    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Approve(int id, [FromBody] ReviewExpenseRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();
        try
        {
            int rows = await _db.ApproveExpenseAsync(id, req.ReviewedBy);
            return rows == 0 ? NotFound($"Expense {id} not found or is not in Submitted status.") : Ok(new { rowsAffected = rows });
        }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
    }

    /// <summary>Reject a submitted expense.</summary>
    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Reject(int id, [FromBody] ReviewExpenseRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();
        try
        {
            int rows = await _db.RejectExpenseAsync(id, req.ReviewedBy);
            return rows == 0 ? NotFound($"Expense {id} not found or is not in Submitted status.") : Ok(new { rowsAffected = rows });
        }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
    }
}
