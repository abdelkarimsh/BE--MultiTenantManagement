using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantManagement.Core.Interfaces;
using MultiTenantManagement.Infrastructure.Features.Order;
using MultiTenantManagement.Infrastructure.Features.Order.Dtos;
using MultiTenantManagement.Infrastructure.Features.Order.Exceptions;
using MultiTenantManagement.Infrastructure.Helpers;

namespace MultiTenantManagement.API.Controllers
{
    [Route("api/tenants/{tenantId}/orders")]
    [ApiController]
    [Authorize(Policy = "TenantAccess", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICurrentUserContext _currentUserContext;
        public OrdersController(IOrderService orderService , ICurrentUserContext currentUserContext)
        {
            _orderService = orderService;
            _currentUserContext = currentUserContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(Guid tenantId, [FromBody] CreateOrderRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var order = await _orderService.CreateOrderAsync(request, tenantId, userId);
                var orderDto = await _orderService.GetOrderByIdAsync(order.Id, tenantId,userId, _currentUserContext.IsTenantAdmin);
                return CreatedAtAction(nameof(GetOrderById), new { tenantId, orderId = order.Id }, orderDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }


        [Authorize(Roles = "TenantAdmin,SystemAdmin")]
        [HttpGet]
        public async Task<ActionResult<PagedResult<OrderListItemDto>>> GetOrders(
            Guid tenantId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = "createdAtUtc",
            [FromQuery] bool isAscending = false,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] Guid? customerId = null,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _orderService.GetOrdersAsync(
                    tenantId,
                    pageNumber,
                    pageSize,
                    sortBy,
                    isAscending,
                    search,
                    status,
                    customerId,
                    ct);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }
        [HttpGet("{orderId:guid}")]
        public async Task<IActionResult> GetOrderById(Guid tenantId, Guid orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var order = await _orderService.GetOrderByIdAsync(orderId, tenantId,userId, _currentUserContext.IsTenantAdmin);
                return Ok(order);
            }
            catch (OrderNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "TenantAdmin")]
        [HttpPost("{orderId:guid}/approve")]
        public async Task<IActionResult> ApproveOrder(Guid tenantId, Guid orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _orderService.ApproveOrderAsync(orderId, tenantId, userId,_currentUserContext.IsTenantAdmin);
                return NoContent();
            }
            catch (OrderNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOrderTransitionException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }
        [Authorize(Roles = "TenantAdmin")]
        [HttpPost("{orderId:guid}/reject")]
        public async Task<IActionResult> RejectOrder(Guid tenantId, Guid orderId, [FromBody] RejectOrderRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _orderService.RejectOrderAsync(orderId, tenantId, userId, _currentUserContext.IsTenantAdmin, request.Reason);
                return NoContent();
            }
            catch (OrderNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOrderTransitionException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "User")]
        [HttpPost("{orderId:guid}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid tenantId, Guid orderId, [FromBody] CancelOrderRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _orderService.CancelOrderAsync(orderId, tenantId, userId, _currentUserContext.IsTenantAdmin, request.Reason);
                return NoContent();
            }
            catch (OrderNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOrderTransitionException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId))
                throw new UnauthorizedAccessException("Invalid user identifier in token.");

            return userId;
        }
    }
}

