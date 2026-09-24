using Asp.Versioning;
using EcomAPI.DTOs.Categories;
using EcomAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomAPI.Controllers;

[Authorize]
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(
        ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var categories = await _categoryService
            .GetAllAsync(cancellationToken);

        return Ok(categories);
    }

    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService
            .GetByIdAsync(id, cancellationToken);

        if (category is null)
            return NotFound();

        return Ok(category);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService
            .CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = category.Id,
                version = "1.0"
            },
            category);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> Update(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService
            .UpdateAsync(
                id,
                request,
                cancellationToken);

        if (category is null)
            return NotFound();

        return Ok(category);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _categoryService
            .DeleteAsync(id, cancellationToken);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}