using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using InventoryManagementAPI.Application.DTOs.Product;
using InventoryManagementAPI.Application.Interfaces;

namespace InventoryManagementAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los productos activos
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ProductResponse>>> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(new { success = true, data = products });
    }

    /// <summary>
    /// Obtiene un producto por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(new { success = false, message = "Producto no encontrado" });

        return Ok(new { success = true, data = product });
    }

    /// <summary>
    /// Crea un nuevo producto (solo Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Validación fallida",
                errors = validation.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
            });
        }

        var product = await _productService.CreateProductAsync(request);
        _logger.LogInformation("[AUDIT] Producto creado: {Name}", request.Name);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, new
        {
            success = true,
            message = "Producto creado exitosamente",
            data = product
        });
    }

    /// <summary>
    /// Actualiza un producto existente (solo Admin)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductResponse>> Update(int id, [FromBody] UpdateProductRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Validación fallida",
                errors = validation.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
            });
        }

        var product = await _productService.UpdateProductAsync(id, request);
        if (product == null)
            return NotFound(new { success = false, message = "Producto no encontrado" });

        _logger.LogInformation("[AUDIT] Producto actualizado: ID={Id}", id);
        return Ok(new { success = true, message = "Producto actualizado exitosamente", data = product });
    }

    /// <summary>
    /// Elimina un producto (solo Admin, soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteProductAsync(id);
        if (!deleted)
            return NotFound(new { success = false, message = "Producto no encontrado" });

        _logger.LogInformation("[AUDIT] Producto eliminado: ID={Id}", id);
        return Ok(new { success = true, message = "Producto eliminado exitosamente" });
    }
}
