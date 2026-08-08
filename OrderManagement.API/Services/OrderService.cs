using OrderManagement.API.DTOs;
using OrderManagement.API.Exceptions;
using OrderManagement.API.Models;
using OrderManagement.API.Repositories;
using OrderManagement.API.UnitOfWork;

namespace OrderManagement.API.Services
{
    public sealed class OrderService : IOrderService
    {
        private const decimal DiscountThreshold = 5000m;
        private const decimal DiscountRate = 0.05m;

        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<OrderResponseDTO> CreateOrderAsync(OrderCreateDTO orderCreateDTO)
        {
            // Ensure that the request object is not null.
            // If it is null, ArgumentNullException is thrown immediately.
            ArgumentNullException.ThrowIfNull(orderCreateDTO);

            // CustomerId must contain a valid positive value.
            if (orderCreateDTO.CustomerId <= 0)
            {
                throw new ArgumentException("CustomerId must be greater than zero.");
            }

            // An order must contain at least one item.
            if (orderCreateDTO.Items is null || orderCreateDTO.Items.Count == 0)
            {
                throw new ArgumentException("Order must contain at least one item.");
            }

            // Retrieve the customer associated with the order from the repository.
            var customer = await _customerRepository.GetByIdAsync(orderCreateDTO.CustomerId);

            // Stop processing when the customer does not exist.
            if (customer is null)
            {
                throw new NotFoundException(
                    $"Customer with ID {orderCreateDTO.CustomerId} " +
                    "was not found.");
            }

            /*
             * Validate the request items and calculate
             * the total requested quantity for every unique product.
             *
             * The dictionary uses ProductId as the key and the total
             * requested quantity as the value.
             */
            var requestedQuantitiesByProductId = new Dictionary<int, int>();

            foreach (var itemDto in orderCreateDTO.Items)
            {
                // Every order item must contain a valid ProductId.
                if (itemDto.ProductId <= 0)
                {
                    throw new ArgumentException("ProductId must be greater than zero.");
                }

                // The requested quantity must be greater than zero.
                if (itemDto.Quantity <= 0)
                {
                    throw new ArgumentException("Quantity must be greater than zero.");
                }

                // Get the quantity already requested for this product.
                // GetValueOrDefault returns 0 when the product is not yet
                // present in the dictionary.
                var previouslyRequestedQuantity =
                    requestedQuantitiesByProductId.GetValueOrDefault(
                        itemDto.ProductId);

                // Add the current quantity to the previously requested quantity.
                // This correctly handles duplicate ProductIds.
                // Example:
                // ProductId 1, Quantity 2
                // ProductId 1, Quantity 3
                // Total requested quantity for ProductId 1 = 5.
                requestedQuantitiesByProductId[itemDto.ProductId] =
                    previouslyRequestedQuantity +
                    itemDto.Quantity;
            }

            // Retrieve and validate every unique product.
            // This dictionary stores the Product objects returned by the repository.
            // It also prevents the same product from being retrieved
            // multiple times when it appears more than once in the request.

            // productsById acts as a temporary product cache
            var productsById = new Dictionary<int, Product>();

            foreach (var requestedProduct in requestedQuantitiesByProductId)
            {
                // Dictionary key represents ProductId.
                var productId = requestedProduct.Key;

                // Dictionary value represents the total quantity requested for that product.
                var totalRequestedQuantity = requestedProduct.Value;

                // Retrieve the product only once for each unique ProductId.
                var product = await _productRepository.GetByIdAsync(productId);

                // Stop processing when the product does not exist.
                if (product is null)
                {
                    throw new NotFoundException($"Product with ID {productId} was not found.");
                }

                // Validate stock against the combined requested quantity.
                // This is important when the same product appears
                // multiple times in the request.
                if (product.Stock < totalRequestedQuantity)
                {
                    throw new InsufficientStockException(
                        $"Insufficient stock for product '{product.Name}'. " +
                        $"Available quantity: {product.Stock}. " +
                        $"Requested quantity: {totalRequestedQuantity}.");
                }

                // Store the validated product for later use.
                productsById.Add(productId, product);
            }

            // All customers, products, quantities, and stock levels
            // have now passed validation.
            // Therefore, it is safe to create the order and apply
            // product-stock changes.
            var order = new Order
            {
                CustomerId = customer.Id,

                // Use the injected TimeProvider instead of DateTime.UtcNow.
                // This makes the order date easy to control during unit testing.
                OrderDate = _timeProvider.GetUtcNow().UtcDateTime,

                //OrderItems will be populated in the next step
                OrderItems = new List<OrderItem>()
            };

            // Stores the total amount before applying the discount.
            decimal baseAmount = 0m;

            // Create the OrderItem entities and calculate
            // the base order amount.
            // We iterate through the original request items so that
            // each request item remains a separate OrderItem.
            foreach (var itemDto in orderCreateDTO.Items)
            {
                // Retrieve the already validated product.
                // No additional repository call is required.
                var product = productsById[itemDto.ProductId];

                // Calculate the amount for the current order item.
                var lineTotal = product.Price * itemDto.Quantity;

                // Create an OrderItem using the product's current price.
                order.OrderItems.Add(
                    new OrderItem
                    {
                        ProductId = product.Id,
                        Product = product,
                        Quantity = itemDto.Quantity,

                        // Store the current product price so that future
                        // price changes do not affect previous orders.
                        UnitPrice = product.Price,
                        LineTotal = lineTotal
                    });

                // Add the current line total to the base order amount.
                baseAmount += lineTotal;
            }

            /*
             * Reduce stock only after every product has passed
             * all validation checks.
             *
             * If any earlier validation fails, execution never reaches
             * this loop. Therefore, no stock is partially reduced.
             */
            foreach (var requestedProduct in requestedQuantitiesByProductId)
            {
                var productId = requestedProduct.Key;
                var totalRequestedQuantity = requestedProduct.Value;

                // Reduce stock once per unique product using the combined
                // requested quantity.
                productsById[productId].Stock -= totalRequestedQuantity;
            }

            // Store the amount before applying the discount.
            order.BaseAmount = baseAmount;

            // Calculate the discount according to the business rule.
            order.DiscountAmount = CalculateDiscount(baseAmount);

            // Calculate the amount that the customer must pay.
            order.TotalAmount = order.BaseAmount - order.DiscountAmount;

            // Add the completed order to the repository.
            // At this point, the order is prepared but not yet saved.
            await _orderRepository.AddAsync(order);

            // Save the new order, order items, and updated product stock.
            await _unitOfWork.SaveChangesAsync();

            // Convert the saved Order entity into the response DTO.
            return MapToOrderResponseDto(order, customer);
        }

        public async Task<OrderResponseDTO?> GetOrderByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "Order ID must be greater than zero.");
            }

            var order = await _orderRepository.GetByIdAsync(id);

            if (order is null)
            {
                return null;
            }

            return MapToOrderResponseDto(
                order,
                order.Customer);
        }

        private static decimal CalculateDiscount(decimal baseAmount)
        {
            if (baseAmount <= DiscountThreshold)
            {
                return 0m;
            }

            return Math.Round(
                baseAmount * DiscountRate,
                2,
                MidpointRounding.AwayFromZero);
        }

        private static OrderResponseDTO MapToOrderResponseDto(Order order, Customer? customer)
        {
            return new OrderResponseDTO
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = customer?.Name ?? string.Empty,
                CustomerEmail = customer?.Email ?? string.Empty,
                OrderDate = order.OrderDate,
                BaseAmount = order.BaseAmount,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,

                OrderItems = order.OrderItems
                    .Select(item => new OrderItemResponseDTO
                    {
                        OrderItemId = item.Id,
                        ProductId = item.ProductId,
                        ProductName =
                            item.Product?.Name ?? string.Empty,
                        Description =
                            item.Product?.Description,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.LineTotal
                    })
                    .ToList()
            };
        }
    }
}
