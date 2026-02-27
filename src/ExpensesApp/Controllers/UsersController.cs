using ExpensesApp.Data;
using ExpensesApp.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
// ReSharper disable once RedundantUsingDirective

namespace ExpensesApp.Controllers;

/// <summary>
/// Users API – CRUD operations for user accounts.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly ExpenseRepository _repo;
    public UsersController(ExpenseRepository repo) => _repo = repo;

    // ── GET /api/users ────────────────────────────────────────────────────────
    [HttpGet]
    [SwaggerOperation(Summary = "Get all users", Description = "Uses dbo.GetAllUsers.")]
    [SwaggerResponse(200, "List of users", typeof(List<User>))]
    public async Task<IActionResult> GetAll()
    {
        var (users, _) = await _repo.GetAllUsersAsync();
        return Ok(users);
    }

    // ── GET /api/users/{id} ───────────────────────────────────────────────────
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get user by ID", Description = "Uses dbo.GetUserById.")]
    [SwaggerResponse(200, "User found", typeof(User))]
    [SwaggerResponse(404, "User not found")]
    public async Task<IActionResult> GetById(int id)
    {
        var (user, _) = await _repo.GetUserByIdAsync(id);
        if (user == null) return NotFound(new { message = $"User {id} not found." });
        return Ok(user);
    }

    // ── POST /api/users ───────────────────────────────────────────────────────
    [HttpPost]
    [SwaggerOperation(Summary = "Create user", Description = "Uses dbo.CreateUser.")]
    [SwaggerResponse(201, "User created")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Email))
            return BadRequest(new { message = "UserName and Email are required." });

        var (newId, err) = await _repo.CreateUserAsync(req);
        if (err != null || newId == 0)
            return StatusCode(503, new { message = err ?? "Failed to create user." });

        return CreatedAtAction(nameof(GetById), new { id = newId },
            new { userId = newId, message = "User created." });
    }

    // ── PUT /api/users/{id} ───────────────────────────────────────────────────
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Update user", Description = "Uses dbo.UpdateUser.")]
    [SwaggerResponse(200, "User updated")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest req)
    {
        var (success, err) = await _repo.UpdateUserAsync(id, req);
        if (!success) return BadRequest(new { message = err ?? "Update failed." });
        return Ok(new { message = "User updated." });
    }

    // ── DELETE /api/users/{id} ────────────────────────────────────────────────
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Deactivate user (soft delete)", Description = "Uses dbo.DeleteUser.")]
    [SwaggerResponse(200, "User deactivated")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, err) = await _repo.DeleteUserAsync(id);
        if (!success) return BadRequest(new { message = err ?? "Delete failed." });
        return Ok(new { message = "User deactivated." });
    }
}
