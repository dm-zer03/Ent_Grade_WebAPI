using System.Security.Claims;
using Asp.Versioning;
using EcomAPI.DTOs.Customers;
using EcomAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomAPI.Controllers;

[Authorize]
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(
        ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var customers = await _customerService
            .GetAllAsync(cancellationToken);

        return Ok(customers);
    }

    // Admin -> any customer
    // Customer -> own customer only
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        var customer = await _customerService
            .GetByIdAsync(
                id,
                currentUserId,
                isAdmin,
                cancellationToken);

        if (customer is null)
            return NotFound();

        return Ok(customer);
    }

    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Create(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerService
            .CreateAsync(
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = customer.Id,
                version = "1.0"
            },
            customer);
    }

    // Admin -> any customer
    // Customer -> own customer only
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> Update(
        int id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        var customer = await _customerService
            .UpdateAsync(
                id,
                request,
                currentUserId,
                isAdmin,
                cancellationToken);

        if (customer is null)
            return NotFound();

        return Ok(customer);
    }

    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _customerService
            .DeleteAsync(
                id,
                cancellationToken);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var currentUserId))
        {
            throw new UnauthorizedAccessException(
                "User ID is missing from the token.");
        }

        return currentUserId;
    }
}