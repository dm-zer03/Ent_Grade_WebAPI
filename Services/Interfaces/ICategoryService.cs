using EcomAPI.DTOs.Categories;

namespace EcomAPI.Services.Interfaces;

public interface ICategoryService
{
    Task<CategoryResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<CategoryResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<CategoryResponse?> UpdateAsync(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}