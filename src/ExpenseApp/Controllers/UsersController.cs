using ExpenseApp.Models;
using ExpenseApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApp.Controllers;

/// <summary>
/// REST API for Users.  All database operations use stored procedures.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IDatabaseService _db;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IDatabaseService db, ILogger<UsersController> logger)
    {
        _db     = db;
        _logger = logger;
    }

    /// <summary>Get all users.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<User>), 200)]
    public async Task<IActionResult> GetAll()
    {
        try   { return Ok(await _db.GetUsersAsync()); }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
    }

    /// <summary>Get a single user by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var user = await _db.GetUserByIdAsync(id);
            return user is null ? NotFound($"User {id} not found.") : Ok(user);
        }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
    }

    /// <summary>Create a new user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();
        try
        {
            int newId = await _db.CreateUserAsync(req);
            return CreatedAtAction(nameof(GetById), new { id = newId }, new { userId = newId });
        }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
    }

    /// <summary>Update an existing user.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();
        try
        {
            int rows = await _db.UpdateUserAsync(id, req);
            return rows == 0 ? NotFound($"User {id} not found.") : Ok(new { rowsAffected = rows });
        }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500); }
    }
}
