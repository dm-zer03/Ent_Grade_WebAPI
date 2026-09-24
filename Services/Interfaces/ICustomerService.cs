using EcomAPI.DTOs.Customers;

namespace EcomAPI.Services.Interfaces;

public interface ICustomerService
{
    Task<CustomerResponse?> GetByIdAsync(
        int id,
        int currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<CustomerResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<CustomerResponse> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<CustomerResponse?> UpdateAsync(
        int id,
        UpdateCustomerRequest request,
        int currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}