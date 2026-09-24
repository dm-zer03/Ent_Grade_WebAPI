using EcomAPI.DTOs.Orders;

namespace EcomAPI.Services.Interfaces;

public interface IOrderService
{
    Task<OrderResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<OrderResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<OrderResponse> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default);
}