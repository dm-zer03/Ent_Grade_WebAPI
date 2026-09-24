
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EcomAPI.Common.Exceptions;
using EcomAPI.DTOs.Categories;
using EcomAPI.Entities;
using EcomAPI.Repositories.Interfaces;
using EcomAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcomAPI.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CategoryResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories
            .Query()
            .AsNoTracking()
            .Where(x => x.Id == id)
            .ProjectTo<CategoryResponse>(
                _mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null)
        {
            _logger.LogWarning(
                "Category {CategoryId} was not found.",
                id);

            return null;
        }

        return category;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories
            .Query()
            .AsNoTracking()
            .ProjectTo<CategoryResponse>(
                _mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return categories;
    }

    public async Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var nameExists = await _unitOfWork.Categories
            .ExistsAsync(
                x => x.Name == request.Name,
                cancellationToken);

        if (nameExists)
        {
            _logger.LogWarning(
                "Category creation failed because the category name already exists.");

            throw new ConflictException(
                "A category with this name already exists.");
        }

        var category = _mapper.Map<Category>(request);

        category.IsActive = true;
        category.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Categories
            .AddAsync(category, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Category {CategoryId} created successfully.",
            category.Id);

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<CategoryResponse?> UpdateAsync(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories
            .GetByIdAsync(id, cancellationToken);

        if (category is null)
        {
            _logger.LogWarning(
                "Category {CategoryId} was not found for update.",
                id);

            return null;
        }

        var nameExists = await _unitOfWork.Categories
            .ExistsAsync(
                x => x.Name == request.Name &&
                     x.Id != id,
                cancellationToken);

        if (nameExists)
        {
            _logger.LogWarning(
                "Category update failed for category {CategoryId} because the name already exists.",
                id);

            throw new ConflictException(
                "A category with this name already exists.");
        }

        _mapper.Map(request, category);

        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Categories.Update(category);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Category {CategoryId} updated successfully.",
            category.Id);

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories
            .GetByIdAsync(id, cancellationToken);

        if (category is null)
        {
            _logger.LogWarning(
                "Category {CategoryId} was not found for deletion.",
                id);

            return false;
        }

        // Soft delete
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Categories.Update(category);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Category {CategoryId} soft-deleted successfully.",
            category.Id);

        return true;
    }
}

