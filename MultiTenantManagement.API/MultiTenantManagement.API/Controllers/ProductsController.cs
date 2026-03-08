using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantManagement.Data.Models;
using MultiTenantManagement.Infrastructure.Features.Product;
using MultiTenantManagement.Infrastructure.Features.Product.Dtos;

namespace MultiTenantManagement.API.Controllers
{
    [Route("api/tenants/{tenantId}/products")]
    [ApiController]
    [Authorize(Policy = "TenantAccess", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/products
        [HttpGet]
        [AllowAnonymous] // Optional: allow public read
        public async Task<IActionResult> GetPaged(
            Guid tenantId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool isAscending = true,
            [FromQuery] string? search = null)
        {
            var result = await _productService.GetPagedAsync(
               tenantId, pageNumber, pageSize, sortBy, isAscending, search);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid tenantId, Guid id)
        {
            var product = await _productService.GetByIdAsync(tenantId, id);
            if (product is null)
                return NotFound();

            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> Create(Guid tenantId, [FromBody]  CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _productService.CreateAsync(tenantId, dto);

            return Ok(created); ;
        }

        // PUT: api/products/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid tenantId, Guid id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _productService.UpdateAsync(tenantId, id, dto);
            if (!success)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid tenantId,Guid id)
        {
            var success = await _productService.DeleteAsync(tenantId,id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
