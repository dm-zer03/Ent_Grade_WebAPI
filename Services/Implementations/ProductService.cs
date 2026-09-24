using AutoMapper;
using AutoMapper.QueryableExtensions;
using EcomAPI.Common.Pagination;
using EcomAPI.DTOs.Products;
using EcomAPI.Entities;
using EcomAPI.Repositories.Interfaces;
using EcomAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcomAPI.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products
            .Query()
            .AsNoTracking()
            .Where(x => x.Id == id)
            .ProjectTo<ProductResponse>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            _logger.LogWarning(
                "Product {ProductId} was not found.",
                id);

            return null;
        }

        return product;
    }

    public async Task<PagedResponse<ProductResponse>> GetAllAsync(
        ProductQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = request.PageNumber < 1
            ? 1
            : request.PageNumber;

        var pageSize = request.PageSize < 1
            ? 20
            : Math.Min(request.PageSize, 100);

        var query = _unitOfWork.Products
            .Query()
            .AsNoTracking()
            .Where(x => x.IsActive);

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(x =>
                x.Name.Contains(request.Search) ||
                x.Description.Contains(request.Search));
        }

        // Price filter
        if (request.MinPrice.HasValue)
        {
            query = query.Where(x =>
                x.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(x =>
                x.Price <= request.MaxPrice.Value);
        }

        // Sorting
        query = request.SortBy.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name),

            "price" => request.SortDescending
                ? query.OrderByDescending(x => x.Price)
                : query.OrderBy(x => x.Price),

            "stock" => request.SortDescending
                ? query.OrderByDescending(x => x.StockQuantity)
                : query.OrderBy(x => x.StockQuantity),

            _ => request.SortDescending
                ? query.OrderByDescending(x => x.Id)
                : query.OrderBy(x => x.Id)
        };

        var totalCount = await query
            .CountAsync(cancellationToken);

        // Projection happens in SQL.
        // Only fields required by ProductResponse are selected.
        var products = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<ProductResponse>(
                _mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        _logger.LogDebug(
            "Retrieved {ProductCount} products. Page: {PageNumber}, PageSize: {PageSize}, TotalCount: {TotalCount}.",
            products.Count,
            pageNumber,
            pageSize,
            totalCount);

        return new PagedResponse<ProductResponse>
        {
            Items = products,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = _mapper.Map<Product>(request);

        product.IsActive = true;
        product.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Products
            .AddAsync(product, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Product {ProductId} created successfully.",
            product.Id);

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse?> UpdateAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products
            .GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            _logger.LogWarning(
                "Product {ProductId} was not found for update.",
                id);

            return null;
        }

        _mapper.Map(request, product);

        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Product {ProductId} updated successfully.",
            product.Id);

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products
            .GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            _logger.LogWarning(
                "Product {ProductId} was not found for deletion.",
                id);

            return false;
        }

        // Soft delete
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Product {ProductId} soft-deleted successfully.",
            product.Id);

        return true;
    }
}
