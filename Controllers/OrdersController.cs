using Asp.Versioning;
using EcomAPI.DTOs.Orders;
using EcomAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomAPI.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(
        IOrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var orders = await _orderService
            .GetAllAsync(cancellationToken);

        return Ok(orders);
    }
    
    [Authorize] 
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var order = await _orderService
            .GetByIdAsync(id, cancellationToken);

        if (order is null)
            return NotFound();

        return Ok(order);
    }

    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _orderService
            .CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = order.Id,
                version = "1.0"
            },
            order);
    }
}