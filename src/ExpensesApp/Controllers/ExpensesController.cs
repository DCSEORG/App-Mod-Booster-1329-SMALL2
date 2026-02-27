using ExpensesApp.Data;
using ExpensesApp.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
// ReSharper disable once RedundantUsingDirective

namespace ExpensesApp.Controllers;

/// <summary>
/// Expenses API – CRUD and workflow operations for expense claims.
/// All operations use stored procedures; no direct SQL is executed in application code.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExpensesController : ControllerBase
{
    private readonly ExpenseRepository _repo;
    public ExpensesController(ExpenseRepository repo) => _repo = repo;

    // ── GET /api/expenses ─────────────────────────────────────────────────────
    [HttpGet]
    [SwaggerOperation(
        Summary     = "Get all expenses",
        Description = "Returns all expense claims. Uses stored procedure dbo.GetAllExpenses.")]
    [SwaggerResponse(200, "List of expenses", typeof(List<Expense>))]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        if (!string.IsNullOrEmpty(status))
        {
            var (byStatus, _) = await _repo.GetExpensesByStatusAsync(status);
            return Ok(byStatus);
        }
        var (expenses, _) = await _repo.GetAllExpensesAsync();
        return Ok(expenses);
    }

    // ── GET /api/expenses/{id} ────────────────────────────────────────────────
    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary     = "Get expense by ID",
        Description = "Returns a single expense. Uses stored procedure dbo.GetExpenseById.")]
    [SwaggerResponse(200, "Expense found",  typeof(Expense))]
    [SwaggerResponse(404, "Expense not found")]
    public async Task<IActionResult> GetById(int id)
    {
        var (expense, _) = await _repo.GetExpenseByIdAsync(id);
        if (expense == null) return NotFound(new { message = $"Expense {id} not found." });
        return Ok(expense);
    }

    // ── GET /api/expenses/user/{userId} ───────────────────────────────────────
    [HttpGet("user/{userId:int}")]
    [SwaggerOperation(
        Summary     = "Get expenses for a user",
        Description = "Returns all expenses for the given user. Uses dbo.GetExpensesByUser.")]
    [SwaggerResponse(200, "List of expenses", typeof(List<Expense>))]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var (expenses, _) = await _repo.GetExpensesByUserAsync(userId);
        return Ok(expenses);
    }

    // ── GET /api/expenses/pending ─────────────────────────────────────────────
    [HttpGet("pending")]
    [SwaggerOperation(
        Summary     = "Get submitted (pending) expenses",
        Description = "Returns expenses awaiting manager review. Uses dbo.GetPendingExpensesForManager.")]
    [SwaggerResponse(200, "Pending expenses", typeof(List<Expense>))]
    public async Task<IActionResult> GetPending()
    {
        var (expenses, _) = await _repo.GetPendingExpensesAsync();
        return Ok(expenses);
    }

    // ── POST /api/expenses ────────────────────────────────────────────────────
    [HttpPost]
    [SwaggerOperation(
        Summary     = "Create a new expense",
        Description = "Creates an expense in Draft status. Uses dbo.CreateExpense.")]
    [SwaggerResponse(201, "Expense created", typeof(object))]
    [SwaggerResponse(400, "Invalid request")]
    public async Task<IActionResult> Create([FromBody] CreateExpenseRequest req)
    {
        if (req.AmountMinor <= 0)
            return BadRequest(new { message = "AmountMinor must be greater than zero." });

        var (newId, err) = await _repo.CreateExpenseAsync(req);
        if (err != null || newId == 0)
            return StatusCode(503, new { message = err ?? "Failed to create expense." });

        return CreatedAtAction(nameof(GetById), new { id = newId },
            new { expenseId = newId, message = "Expense created." });
    }

    // ── PUT /api/expenses/{id} ────────────────────────────────────────────────
    [HttpPut("{id:int}")]
    [SwaggerOperation(
        Summary     = "Update a draft expense",
        Description = "Updates an expense (Draft status only). Uses dbo.UpdateExpense.")]
    [SwaggerResponse(200, "Expense updated")]
    [SwaggerResponse(400, "Invalid request or expense not in Draft")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseRequest req)
    {
        var (success, err) = await _repo.UpdateExpenseAsync(id, req);
        if (!success)
            return BadRequest(new { message = err ?? "Update failed – expense may not be in Draft status." });
        return Ok(new { message = "Expense updated." });
    }

    // ── POST /api/expenses/{id}/submit ────────────────────────────────────────
    [HttpPost("{id:int}/submit")]
    [SwaggerOperation(
        Summary     = "Submit expense for approval",
        Description = "Transitions a Draft expense to Submitted. Uses dbo.SubmitExpense.")]
    [SwaggerResponse(200, "Expense submitted")]
    [SwaggerResponse(400, "Cannot submit")]
    public async Task<IActionResult> Submit(int id)
    {
        var (success, err) = await _repo.SubmitExpenseAsync(id);
        if (!success)
            return BadRequest(new { message = err ?? "Submit failed – expense may not be in Draft status." });
        return Ok(new { message = "Expense submitted for approval." });
    }

    // ── POST /api/expenses/{id}/approve ──────────────────────────────────────
    [HttpPost("{id:int}/approve")]
    [SwaggerOperation(
        Summary     = "Approve a submitted expense",
        Description = "Manager action: approve. Uses dbo.ApproveExpense.")]
    [SwaggerResponse(200, "Expense approved")]
    [SwaggerResponse(400, "Cannot approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ReviewExpenseRequest req)
    {
        var (success, err) = await _repo.ApproveExpenseAsync(id, req.ReviewedBy);
        if (!success)
            return BadRequest(new { message = err ?? "Approve failed – expense may not be in Submitted status." });
        return Ok(new { message = "Expense approved." });
    }

    // ── POST /api/expenses/{id}/reject ────────────────────────────────────────
    [HttpPost("{id:int}/reject")]
    [SwaggerOperation(
        Summary     = "Reject a submitted expense",
        Description = "Manager action: reject. Uses dbo.RejectExpense.")]
    [SwaggerResponse(200, "Expense rejected")]
    [SwaggerResponse(400, "Cannot reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ReviewExpenseRequest req)
    {
        var (success, err) = await _repo.RejectExpenseAsync(id, req.ReviewedBy);
        if (!success)
            return BadRequest(new { message = err ?? "Reject failed – expense may not be in Submitted status." });
        return Ok(new { message = "Expense rejected." });
    }

    // ── DELETE /api/expenses/{id} ─────────────────────────────────────────────
    [HttpDelete("{id:int}")]
    [SwaggerOperation(
        Summary     = "Delete a draft expense",
        Description = "Deletes an expense (Draft only). Uses dbo.DeleteExpense.")]
    [SwaggerResponse(200, "Expense deleted")]
    [SwaggerResponse(400, "Cannot delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, err) = await _repo.DeleteExpenseAsync(id);
        if (!success)
            return BadRequest(new { message = err ?? "Delete failed – expense may not be in Draft status." });
        return Ok(new { message = "Expense deleted." });
    }

    // ── GET /api/expenses/summary/category ───────────────────────────────────
    [HttpGet("summary/category")]
    [SwaggerOperation(
        Summary     = "Expense summary by category",
        Description = "Returns totals grouped by category. Uses dbo.GetExpenseSummaryByCategory.")]
    [SwaggerResponse(200, "Category summaries", typeof(List<ExpenseSummary>))]
    public async Task<IActionResult> SummaryByCategory()
    {
        var (summaries, _) = await _repo.GetSummaryByCategoryAsync();
        return Ok(summaries);
    }

    // ── GET /api/expenses/summary/status ─────────────────────────────────────
    [HttpGet("summary/status")]
    [SwaggerOperation(
        Summary     = "Expense summary by status",
        Description = "Returns totals grouped by status. Uses dbo.GetExpenseSummaryByStatus.")]
    [SwaggerResponse(200, "Status summaries", typeof(List<ExpenseSummary>))]
    public async Task<IActionResult> SummaryByStatus()
    {
        var (summaries, _) = await _repo.GetSummaryByStatusAsync();
        return Ok(summaries);
    }
}
