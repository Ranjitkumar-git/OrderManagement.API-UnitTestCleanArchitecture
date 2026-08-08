using Microsoft.AspNetCore.Mvc;
using OrderManagement.API.DTOs;
using OrderManagement.API.Exceptions;
using OrderManagement.API.Services;

namespace OrderManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(OrderResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderResponseDTO>> CreateOrder(
            [FromBody] OrderCreateDTO orderCreateDTO)
        {
            try
            {
                var order = await _orderService
                    .CreateOrderAsync(orderCreateDTO);

                return CreatedAtAction(
                    nameof(GetOrderById),
                    new { id = order.OrderId },
                    order);
            }
            catch (NotFoundException exception)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = exception.Message
                });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = exception.Message
                });
            }
            catch (InsufficientStockException exception)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = exception.Message
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "An unexpected error occurred while creating an order.");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponseDTO
                    {
                        Message = "An unexpected error occurred."
                    });
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(OrderResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderResponseDTO>> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order is null)
                {
                    return NotFound(new ErrorResponseDTO
                    {
                        Message = $"Order with ID {id} was not found."
                    });
                }

                return Ok(order);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(
                    new ErrorResponseDTO
                    {
                        Message = exception.Message
                    });
            }
            catch (Exception exception)
            {
                _logger.LogError(
                   exception,
                   "An unexpected error occurred while retrieving order {OrderId}.",
                   id);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponseDTO
                    {
                        Message =
                            "An unexpected error occurred."
                    });
            }
        }
    }
}
