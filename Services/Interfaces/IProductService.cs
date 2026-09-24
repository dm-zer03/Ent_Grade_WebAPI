using EcomAPI.Common.Pagination;
using EcomAPI.DTOs.Products;

namespace EcomAPI.Services.Interfaces;

public interface IProductService
{
    Task<ProductResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<ProductResponse>> GetAllAsync(
    ProductQueryRequest request,
    CancellationToken cancellationToken = default);

    Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default);

    Task<ProductResponse?> UpdateAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}