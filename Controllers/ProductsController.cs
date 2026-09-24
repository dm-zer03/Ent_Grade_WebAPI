using Asp.Versioning;
using EcomAPI.Common.Constants;
using EcomAPI.Common.Pagination;
using EcomAPI.Common.Responses;
using EcomAPI.DTOs.Products;
using EcomAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomAPI.Controllers;

[Authorize]
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(
        IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Gets a paginated list of active products.
    /// </summary>
    /// <param name="request">
    /// Pagination, filtering and sorting options.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>A paginated list of products.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<ProductResponse>>>> GetAll(
        [FromQuery] ProductQueryRequest request,
        CancellationToken cancellationToken)
    {
        var products = await _productService
            .GetAllAsync(request, cancellationToken);

        return Ok(
            ApiResponse<PagedResponse<ProductResponse>>.Ok(
                products,
                "Products retrieved successfully."));
    }

    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    /// <param name="id">Product ID.</param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>The requested product.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _productService
            .GetByIdAsync(id, cancellationToken);

        if (product is null)
            return NotFound();

        return Ok(
            ApiResponse<ProductResponse>.Ok(
                product,
                "Product retrieved successfully."));
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">Product creation details.</param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>The newly created product.</returns>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> Create(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _productService
            .CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = product.Id,
                version = "1.0"
            },
            ApiResponse<ProductResponse>.Ok(
                product,
                "Product created successfully."));
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">Product ID.</param>
    /// <param name="request">Updated product details.</param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>The updated product.</returns>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> Update(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _productService
            .UpdateAsync(
                id,
                request,
                cancellationToken);

        if (product is null)
            return NotFound();

        return Ok(
            ApiResponse<ProductResponse>.Ok(
                product,
                "Product updated successfully."));
    }

    /// <summary>
    /// Soft deletes a product.
    /// </summary>
    /// <param name="id">Product ID.</param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>No content when the product is deleted.</returns>
    [Authorize(Roles = Roles.Admin )]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _productService
            .DeleteAsync(id, cancellationToken);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

