using Moq;
using OrderManagement.API.DTOs;
using OrderManagement.API.Exceptions;
using OrderManagement.API.Models;
using OrderManagement.API.Repositories;
using OrderManagement.API.Services;
using OrderManagement.API.UnitOfWork;
using System.Runtime.Intrinsics.X86;
using Xunit;

namespace OrderManagement.Tests.Services
{
    public class OrderServiceTests
    {
        // These fields represent mocked dependencies.
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<TimeProvider> _timeProviderMock;

        // This field represents the real class being tested.
        private readonly OrderService _orderService;


        public OrderServiceTests()
        {
            // Create the mock dependencies
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _timeProviderMock = new Mock<TimeProvider>();

            // The .Object property returns the generated mocked implementation 
            // of the corresponding interface.
            _orderService = new OrderService(
                _orderRepositoryMock.Object,
                _productRepositoryMock.Object,
                _customerRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _timeProviderMock.Object);
        }

        //Test 1: Test a Null Order Request

        [Fact]
        public async Task CreateOrderAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            OrderCreateDTO request = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _orderService.CreateOrderAsync(request));
        }

        //Test 2: Test an Invalid Customer ID

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task CreateOrderAsync_WhenCustomerIdIsInvalid_ThrowsArgumentException(int customerId)
        {
            // Arrange
            var request = new OrderCreateDTO
            {
                CustomerId = customerId,
                Items = new List<OrderItemCreateDTO>
                {
                    new()
                    {
                        ProductId = 1,
                        Quantity = 1
                    }
                }
            };

            // Act
            // Assert.ThrowsAsync verifies that the asynchronous operation throws
            // an ArgumentException.
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => _orderService.CreateOrderAsync(request));

            // Assert
            Assert.Equal(
                "CustomerId must be greater than zero.",
                exception.Message);

            // The following verification checks that the customer repository was never called
            // It.IsAny<int>() means any integer value.
            // Times.Never means the method must not have been called
            _customerRepositoryMock.Verify(
                repository => repository.GetByIdAsync(
                    It.IsAny<int>()),
                    Times.Never);
        }
        //Test 3: Test an Order Without Items
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateOrderAsync_WhenOrderHasNoItems_ThrowsArgumentException(bool useNullItems)
        {
            // Arrange
            var request = new OrderCreateDTO
            {
                CustomerId = 1,
                Items = useNullItems
                    ? null!
                    : new List<OrderItemCreateDTO>()
            };

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => _orderService.CreateOrderAsync(request));

            // Assert
            Assert.Equal(
                "Order must contain at least one item.",
                exception.Message);

            _customerRepositoryMock.Verify(
                repository => repository.GetByIdAsync(
                    It.IsAny<int>()),
                Times.Never);
        }

        //Test 4: Test a Missing Customer
        [Fact]
        public async Task CreateOrderAsync_WhenCustomerDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            const int customerId = 100;

            var request = new OrderCreateDTO
            {
                CustomerId = customerId,
                Items = new List<OrderItemCreateDTO>
                {
                    new()
                    {
                        ProductId = 1,
                        Quantity = 1
                    }
                }
            };

            // The following setup tells the mock reposiory to return null asynchronously
            // The explicit cast is useful because it tells Moq that
            // the returned value is a nullable Customer
            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act
            var exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => _orderService.CreateOrderAsync(request));

            // Assert
            Assert.Equal(
                "Customer with ID 100 was not found.",
                exception.Message);

            // Verify that GetByIdAsync was called exactly once
            _customerRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(customerId),
                Times.Once);

            // No product should be retrieved
            _productRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(It.IsAny<int>()),
                Times.Never);

            // No order should be added
            _orderRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(It.IsAny<Order>()),
                Times.Never);

            // No changes should be saved
            _unitOfWorkMock.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(),
                Times.Never);
        }
        //Test 5: Test an Invalid Product ID

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-25)]
        public async Task CreateOrderAsync_WhenProductIdIsInvalid_ThrowsArgumentException(int productId)
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "Pranaya Rout",
                Email = "pranaya@example.com"
            };

            var request = new OrderCreateDTO
            {
                CustomerId = customer.Id,
                Items = new List<OrderItemCreateDTO>
                {
                    new()
                    {
                        ProductId = productId,
                        Quantity = 1
                    }
                }
            };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(customer.Id))
                .ReturnsAsync(customer);

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => _orderService.CreateOrderAsync(request));

            // Assert
            Assert.Equal(
                "ProductId must be greater than zero.",
                exception.Message);

            _productRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(It.IsAny<int>()),
                Times.Never);

            _orderRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(It.IsAny<Order>()),
                Times.Never);

            _unitOfWorkMock.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(),
                Times.Never);
        }
        //Test 6: Test an Invalid Quantity
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public async Task CreateOrderAsync_WhenQuantityIsInvalid_ThrowsArgumentException(int quantity)
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "Pranaya Rout",
                Email = "pranaya@example.com"
            };

            var request = new OrderCreateDTO
            {
                CustomerId = customer.Id,
                Items = new List<OrderItemCreateDTO>
                {
                    new()
                    {
                        ProductId = 1,
                        Quantity = quantity
                    }
                }
            };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(customer.Id))
                .ReturnsAsync(customer);

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => _orderService.CreateOrderAsync(request));

            // Assert
            Assert.Equal(
                "Quantity must be greater than zero.",
                exception.Message);

            _productRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(It.IsAny<int>()),
                Times.Never);

            _orderRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(It.IsAny<Order>()),
                Times.Never);

            _unitOfWorkMock.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(),
                Times.Never);
        }

        //Test 7: Test a Missing Product
        [Fact]
        public async Task CreateOrderAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "Pranaya Rout",
                Email = "pranaya@example.com"
            };

            const int productId = 999;

            var request = new OrderCreateDTO
            {
                CustomerId = customer.Id,
                Items = new List<OrderItemCreateDTO>
                {
                    new()
                    {
                        ProductId = productId,
                        Quantity = 1
                    }
                }
            };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(customer.Id))
                .ReturnsAsync(customer);

            _productRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act
            var exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => _orderService.CreateOrderAsync(request));

            // Assert
            Assert.Equal(
                "Product with ID 999 was not found.",
                exception.Message);

            _productRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(productId),
                Times.Once);

            _orderRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(It.IsAny<Order>()),
                Times.Never);

            _unitOfWorkMock.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(),
                Times.Never);
        }



        //Test 8: Test Insufficient Product Stock
        [Fact]
        public async Task CreateOrderAsync_WhenStockIsInsufficient_ThrowsInsufficientStockException()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "Pranaya Rout",
                Email = "pranaya@example.com"
            };

            var product = new Product
            {
                Id = 1,
                Name = "Laptop",
                Price = 60000m,
                Stock = 2,
                Description = "High performance laptop"
            };

            var request = new OrderCreateDTO
            {
                CustomerId = customer.Id,
                Items = new List<OrderItemCreateDTO>
                {
                    new()
                    {
                        ProductId = product.Id,
                        Quantity = 3
                    }
                }
            };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(customer.Id))
                .ReturnsAsync(customer);

            _productRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(product.Id))
                .ReturnsAsync(product);

            // Act
            var exception =
                await Assert.ThrowsAsync<InsufficientStockException>(
                    () => _orderService.CreateOrderAsync(request));

            // Assert
            Assert.Equal(
                "Insufficient stock for product 'Laptop'. " +
                "Available quantity: 2. " +
                "Requested quantity: 3.",
                exception.Message);

            // The stock must remain unchanged because the order was rejected.
            Assert.Equal(2, product.Stock);

            _orderRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(It.IsAny<Order>()),
                Times.Never);

            _unitOfWorkMock.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(),
                Times.Never);
        }
        //Test 9: Test the Discount Boundary
        [Fact]
        public async Task CreateOrderAsync_WhenBaseAmountEquals5000_DoesNotApplyDiscount()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "Pranaya Rout",
                Email = "pranaya@example.com"
            };

            var product = new Product
            {
                Id = 1,
                Name = "Keyboard",
                Price = 2500m,
                Stock = 10
            };

            var request = new OrderCreateDTO
            {
                CustomerId = customer.Id,
                Items = new List<OrderItemCreateDTO>
                {
                    new()
                    {
                        ProductId = product.Id,
                        Quantity = 2
                    }
                }
            };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(customer.Id))
                .ReturnsAsync(customer);

            _productRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(product.Id))
                .ReturnsAsync(product);

            _timeProviderMock
                .Setup(timeProvider =>
                    timeProvider.GetUtcNow())
                .Returns(
                    new DateTimeOffset(
                        2026,
                        7,
                        27,
                        10,
                        30,
                        0,
                        TimeSpan.Zero));

            _orderRepositoryMock
                .Setup(repository =>
                    repository.AddAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(unitOfWork =>
                    unitOfWork.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result =
                await _orderService.CreateOrderAsync(request);

            // Assert
            Assert.Equal(5000m, result.BaseAmount);
            Assert.Equal(0m, result.DiscountAmount);
            Assert.Equal(5000m, result.TotalAmount);
        }
        //Test 10: Test the 5% Discount

        [Fact]
        public async Task CreateOrderAsync_WhenBaseAmountExceeds5000_AppliesFivePercentDiscount()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "Pranaya Rout",
                Email = "pranaya@example.com"
            };

            var product = new Product
            {
                Id = 1,
                Name = "Monitor",
                Price = 3000m,
                Stock = 10
            };

            var request = new OrderCreateDTO
            {
                CustomerId = customer.Id,
                Items = new List<OrderItemCreateDTO>
                {
                    new()
                    {
                        ProductId = product.Id,
                        Quantity = 2
                    }
                }
            };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(customer.Id))
                .ReturnsAsync(customer);

            _productRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(product.Id))
                .ReturnsAsync(product);

            _timeProviderMock
                .Setup(timeProvider =>
                    timeProvider.GetUtcNow())
                .Returns(
                    new DateTimeOffset(
                        2026,
                        7,
                        27,
                        10,
                        30,
                        0,
                        TimeSpan.Zero));

            _orderRepositoryMock
                .Setup(repository =>
                    repository.AddAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(unitOfWork =>
                    unitOfWork.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result =
                await _orderService.CreateOrderAsync(request);

            // Assert
            Assert.Equal(6000m, result.BaseAmount);
            Assert.Equal(300m, result.DiscountAmount);
            Assert.Equal(5700m, result.TotalAmount);
        }

        //Test 11: Test Complete Successful Order Creation
        [Fact]
        public async Task CreateOrderAsync_WhenRequestIsValid_CreatesAndReturnsOrder()
        {
            // Arrange
            var fixedDate = new DateTimeOffset(
                2026,
                7,
                27,
                10,
                30,
                0,
                TimeSpan.Zero);

            var customer = new Customer
            {
                Id = 1,
                Name = "Pranaya Rout",
                Email = "pranaya@example.com"
            };

            var laptop = new Product
            {
                Id = 1,
                Name = "Laptop",
                Price = 60000m,
                Stock = 20,
                Description = "High performance laptop"
            };

            var mouse = new Product
            {
                Id = 3,
                Name = "Wireless Mouse",
                Price = 1500m,
                Stock = 100,
                Description = "Ergonomic wireless mouse"
            };

            var request = new OrderCreateDTO
            {
                CustomerId = customer.Id,
                Items = new List<OrderItemCreateDTO>
        {
            new()
            {
                ProductId = laptop.Id,
                Quantity = 2
            },
            new()
            {
                ProductId = mouse.Id,
                Quantity = 3
            }
        }
            };

            // Return the expected customer when the service searches
            // for the customer by ID.
            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(customer.Id))
                .ReturnsAsync(customer);

            // Return the laptop when the service requests Product ID 1.
            _productRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(laptop.Id))
                .ReturnsAsync(laptop);

            // Return the wireless mouse when the service requests Product ID 3.
            _productRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(mouse.Id))
                .ReturnsAsync(mouse);

            // Return a fixed date instead of the actual system date.
            // This makes the test predictable and repeatable.
            _timeProviderMock
                .Setup(timeProvider =>
                    timeProvider.GetUtcNow())
                .Returns(fixedDate);

            // This variable captures the actual Order object created by
            // OrderService and passed to the order repository.
            Order? capturedOrder = null;

            _orderRepositoryMock
                .Setup(repository =>
                    repository.AddAsync(It.IsAny<Order>()))

                // This callback runs when OrderService calls AddAsync.
                //
                // The 'order' parameter is the same Order object created
                // inside OrderService and passed to the repository.
                //
                // We capture that object so that we can verify its values
                // after CreateOrderAsync has completed.
                .Callback<Order>(order =>
                {
                    capturedOrder = order;
                })

                // AddAsync returns Task, so the mock returns a completed task.
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(unitOfWork =>
                    unitOfWork.SaveChangesAsync())

                // In the real application, SQL Server-generated IDs normally
                // become available after SaveChangesAsync completes.
                //
                // Because this unit test does not use EF Core or SQL Server,
                // this callback simulates the database-generated IDs.
                //
                // At this point, capturedOrder contains the Order object that
                // was previously passed to AddAsync.
                .Callback(() =>
                {
                    capturedOrder!.Id = 101;

                    capturedOrder.OrderItems[0].Id = 1001;
                    capturedOrder.OrderItems[1].Id = 1002;
                })

                // Simulate one successful database save operation.
                .ReturnsAsync(1);

            // Act
            var result =
                await _orderService.CreateOrderAsync(request);

            // Assert: Verify the order captured by the repository
            Assert.NotNull(capturedOrder);

            Assert.Equal(101, capturedOrder.Id);
            Assert.Equal(customer.Id, capturedOrder.CustomerId);
            Assert.Equal(
                fixedDate.UtcDateTime,
                capturedOrder.OrderDate);

            // Laptop: 60,000 × 2 = 120,000
            // Mouse:   1,500 × 3 =   4,500
            // Base amount:          124,500
            Assert.Equal(
                124500m,
                capturedOrder.BaseAmount);

            // Discount: 124,500 × 5% = 6,225
            Assert.Equal(
                6225m,
                capturedOrder.DiscountAmount);

            // Total amount: 124,500 − 6,225 = 118,275
            Assert.Equal(
                118275m,
                capturedOrder.TotalAmount);

            // Assert: Verify the created order items
            Assert.Equal(
                2,
                capturedOrder.OrderItems.Count);

            var laptopOrderItem =
                Assert.Single(
                    capturedOrder.OrderItems,
                    item => item.ProductId == laptop.Id);

            Assert.Equal(1001, laptopOrderItem.Id);
            Assert.Equal(2, laptopOrderItem.Quantity);
            Assert.Equal(60000m, laptopOrderItem.UnitPrice);
            Assert.Equal(120000m, laptopOrderItem.LineTotal);
            Assert.Same(laptop, laptopOrderItem.Product);

            var mouseOrderItem =
                Assert.Single(
                    capturedOrder.OrderItems,
                    item => item.ProductId == mouse.Id);

            Assert.Equal(1002, mouseOrderItem.Id);
            Assert.Equal(3, mouseOrderItem.Quantity);
            Assert.Equal(1500m, mouseOrderItem.UnitPrice);
            Assert.Equal(4500m, mouseOrderItem.LineTotal);
            Assert.Same(mouse, mouseOrderItem.Product);

            // Assert: Verify product stock reduction
            Assert.Equal(18, laptop.Stock);
            Assert.Equal(97, mouse.Stock);

            // Assert: Verify the returned order response
            Assert.Equal(101, result.OrderId);
            Assert.Equal(customer.Id, result.CustomerId);
            Assert.Equal(customer.Name, result.CustomerName);
            Assert.Equal(customer.Email, result.CustomerEmail);
            Assert.Equal(
                fixedDate.UtcDateTime,
                result.OrderDate);

            Assert.Equal(124500m, result.BaseAmount);
            Assert.Equal(6225m, result.DiscountAmount);
            Assert.Equal(118275m, result.TotalAmount);

            Assert.Equal(2, result.OrderItems.Count);

            // Assert: Verify laptop response-item mapping
            var laptopResponseItem =
                Assert.Single(
                    result.OrderItems,
                    item => item.ProductId == laptop.Id);

            Assert.Equal(
                1001,
                laptopResponseItem.OrderItemId);

            Assert.Equal(
                "Laptop",
                laptopResponseItem.ProductName);

            Assert.Equal(
                "High performance laptop",
                laptopResponseItem.Description);

            Assert.Equal(
                2,
                laptopResponseItem.Quantity);

            Assert.Equal(
                60000m,
                laptopResponseItem.UnitPrice);

            Assert.Equal(
                120000m,
                laptopResponseItem.LineTotal);

            // Assert: Verify mouse response-item mapping
            var mouseResponseItem =
                Assert.Single(
                    result.OrderItems,
                    item => item.ProductId == mouse.Id);

            Assert.Equal(
                1002,
                mouseResponseItem.OrderItemId);

            Assert.Equal(
                "Wireless Mouse",
                mouseResponseItem.ProductName);

            Assert.Equal(
                "Ergonomic wireless mouse",
                mouseResponseItem.Description);

            Assert.Equal(
                3,
                mouseResponseItem.Quantity);

            Assert.Equal(
                1500m,
                mouseResponseItem.UnitPrice);

            Assert.Equal(
                4500m,
                mouseResponseItem.LineTotal);

            // Assert: Verify dependency interactions
            _customerRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(customer.Id),
                Times.Once);

            _productRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(laptop.Id),
                Times.Once);

            _productRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(mouse.Id),
                Times.Once);

            _orderRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(
                        It.Is<Order>(order =>
                            order.CustomerId == customer.Id &&
                            order.OrderItems.Count == 2)),
                Times.Once);

            _unitOfWorkMock.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(),
                Times.Once);
        }

        //Test 13: Later Item Failure Does Not Reduce Earlier Stock
        [Fact]
        public async Task CreateOrderAsync_WhenLaterProductHasInsufficientStock_DoesNotReduceAnyStock()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "Pranaya Rout",
                Email = "pranaya@example.com"
            };

            var laptop = new Product
            {
                Id = 1,
                Name = "Laptop",
                Price = 60000m,
                Stock = 20
            };

            var mouse = new Product
            {
                Id = 2,
                Name = "Wireless Mouse",
                Price = 1500m,
                Stock = 2
            };

            var request = new OrderCreateDTO
            {
                CustomerId = customer.Id,
                Items = new List<OrderItemCreateDTO>
                {
                    new()
                    {
                        ProductId = laptop.Id,
                        Quantity = 2
                    },
                    new()
                    {
                        ProductId = mouse.Id,
                        Quantity = 3
                    }
                }
            };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(customer.Id))
                .ReturnsAsync(customer);

            _productRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(laptop.Id))
                .ReturnsAsync(laptop);

            _productRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(mouse.Id))
                .ReturnsAsync(mouse);

            // Act
            var exception =
                await Assert.ThrowsAsync<InsufficientStockException>(
                    () => _orderService.CreateOrderAsync(request));

            // Assert
            Assert.Equal(
                "Insufficient stock for product 'Wireless Mouse'. " +
                "Available quantity: 2. " +
                "Requested quantity: 3.",
                exception.Message);

            // Neither product stock should be changed because
            // validation did not complete successfully.
            Assert.Equal(20, laptop.Stock);
            Assert.Equal(2, mouse.Stock);

            _orderRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(It.IsAny<Order>()),
                Times.Never);

            _unitOfWorkMock.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(),
                Times.Never);
        }

        //Test 14: Duplicate Products Use the Combined Quantity (Failed Scenario)
        [Fact]
        public async Task CreateOrderAsync_WhenProductAppearsMultipleTimes_ValidatesCombinedQuantity()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "Pranaya Rout",
                Email = "pranaya@example.com"
            };

            var laptop = new Product
            {
                Id = 1,
                Name = "Laptop",
                Price = 60000m,
                Stock = 10
            };

            var request = new OrderCreateDTO
            {
                CustomerId = customer.Id,
                Items = new List<OrderItemCreateDTO>
        {
            new()
            {
                ProductId = laptop.Id,
                Quantity = 6
            },
            new()
            {
                ProductId = laptop.Id,
                Quantity = 5
            }
        }
            };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(customer.Id))
                .ReturnsAsync(customer);

            _productRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(laptop.Id))
                .ReturnsAsync(laptop);

            // Act
            var exception =
                await Assert.ThrowsAsync<InsufficientStockException>(
                    () => _orderService.CreateOrderAsync(request));

            // Assert
            Assert.Equal(
                "Insufficient stock for product 'Laptop'. " +
                "Available quantity: 10. " +
                "Requested quantity: 11.",
                exception.Message);

            Assert.Equal(10, laptop.Stock);

            // The product should be retrieved only once even though
            // it appears twice in the request.
            _productRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(laptop.Id),
                Times.Once);

            _orderRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(It.IsAny<Order>()),
                Times.Never);

            _unitOfWorkMock.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(),
                Times.Never);
        }
        //Test 15: Duplicate Products Use the Combined Quantity (Success Scenario)
        [Fact]
        public async Task CreateOrderAsync_WhenProductAppearsMultipleTimes_UsesCombinedQuantityAndCreatesOrder()
        {
            // Arrange
            var fixedDate =
                new DateTimeOffset(
                    2026,
                    7,
                    27,
                    10,
                    30,
                    0,
                    TimeSpan.Zero);

            var customer = new Customer
            {
                Id = 1,
                Name = "Pranaya Rout",
                Email = "pranaya@example.com"
            };

            var laptop = new Product
            {
                Id = 1,
                Name = "Laptop",
                Price = 2000m,
                Stock = 10,
                Description = "High performance laptop"
            };

            /*
             * The same product appears twice in the request.
             *
             * First quantity:  2
             * Second quantity: 3
             * Combined quantity: 5
             */
            var request = new OrderCreateDTO
            {
                CustomerId = customer.Id,
                Items = new List<OrderItemCreateDTO>
        {
            new()
            {
                ProductId = laptop.Id,
                Quantity = 2
            },
            new()
            {
                ProductId = laptop.Id,
                Quantity = 3
            }
        }
            };

            _customerRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(customer.Id))
                .ReturnsAsync(customer);

            /*
             * Although the product appears twice in the request,
             * the service should retrieve it only once.
             */
            _productRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(laptop.Id))
                .ReturnsAsync(laptop);

            _timeProviderMock
                .Setup(timeProvider =>
                    timeProvider.GetUtcNow())
                .Returns(fixedDate);

            Order? capturedOrder = null;

            _orderRepositoryMock
                .Setup(repository =>
                    repository.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(order =>
                {
                    // Capture the Order passed to the repository
                    // so that its values can be verified later.
                    capturedOrder = order;
                })
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(unitOfWork =>
                    unitOfWork.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result =
                await _orderService.CreateOrderAsync(request);

            // Assert: Verify that the order was created.
            Assert.NotNull(capturedOrder);

            Assert.Equal(customer.Id, capturedOrder.CustomerId);
            Assert.Equal(
                fixedDate.UtcDateTime,
                capturedOrder.OrderDate);

            /*
             * The request contains two separate order-item lines.
             * Therefore, two OrderItem objects should be created.
             */
            Assert.Equal(
                2,
                capturedOrder.OrderItems.Count);

            // Verify the first order-item line.
            Assert.Equal(
                laptop.Id,
                capturedOrder.OrderItems[0].ProductId);

            Assert.Equal(
                2,
                capturedOrder.OrderItems[0].Quantity);

            Assert.Equal(
                2000m,
                capturedOrder.OrderItems[0].UnitPrice);

            Assert.Equal(
                4000m,
                capturedOrder.OrderItems[0].LineTotal);

            // Verify the second order-item line.
            Assert.Equal(
                laptop.Id,
                capturedOrder.OrderItems[1].ProductId);

            Assert.Equal(
                3,
                capturedOrder.OrderItems[1].Quantity);

            Assert.Equal(
                2000m,
                capturedOrder.OrderItems[1].UnitPrice);

            Assert.Equal(
                6000m,
                capturedOrder.OrderItems[1].LineTotal);

            /*
             * Amount calculation:
             *
             * First line:  2,000 × 2 = 4,000
             * Second line: 2,000 × 3 = 6,000
             * Base amount:            10,000
             * Discount: 10,000 × 5% =   500
             * Total amount:            9,500
             */
            Assert.Equal(
                10000m,
                capturedOrder.BaseAmount);

            Assert.Equal(
                500m,
                capturedOrder.DiscountAmount);

            Assert.Equal(
                9500m,
                capturedOrder.TotalAmount);

            /*
             * The combined requested quantity is five.
             *
             * Original stock: 10
             * Requested stock: 5
             * Remaining stock: 5
             */
            Assert.Equal(5, laptop.Stock);

            // Verify the response DTO.
            Assert.Equal(
                customer.Id,
                result.CustomerId);

            Assert.Equal(
                10000m,
                result.BaseAmount);

            Assert.Equal(
                500m,
                result.DiscountAmount);

            Assert.Equal(
                9500m,
                result.TotalAmount);

            Assert.Equal(
                2,
                result.OrderItems.Count);

            // Verify the two separate response-item lines.
            Assert.Collection(
                result.OrderItems,
                firstItem =>
                {
                    Assert.Equal(laptop.Id, firstItem.ProductId);
                    Assert.Equal(2, firstItem.Quantity);
                    Assert.Equal(2000m, firstItem.UnitPrice);
                    Assert.Equal(4000m, firstItem.LineTotal);
                },
                secondItem =>
                {
                    Assert.Equal(laptop.Id, secondItem.ProductId);
                    Assert.Equal(3, secondItem.Quantity);
                    Assert.Equal(2000m, secondItem.UnitPrice);
                    Assert.Equal(6000m, secondItem.LineTotal);
                });

            /*
             * The product repository must be called only once
             * because the service retrieves each unique product once.
             */
            _productRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(laptop.Id),
                Times.Once);

            // The completed order must be added once.
            _orderRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(
                        It.Is<Order>(order =>
                            order.OrderItems.Count == 2 &&
                            order.OrderItems[0].Quantity == 2 &&
                            order.OrderItems[1].Quantity == 3)),
                Times.Once);

            // The order and updated stock must be saved once.
            _unitOfWorkMock.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(),
                Times.Once);
        }

        //Test 16: Test an Invalid Order ID
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task GetOrderByIdAsync_WhenIdIsInvalid_ThrowsArgumentException(int orderId)
        {
            // Arrange is provided by InlineData.

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => _orderService.GetOrderByIdAsync(orderId));

            // Assert
            Assert.Equal(
                "Order ID must be greater than zero.",
                exception.Message);

            _orderRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(It.IsAny<int>()),
                Times.Never);
        }

        //Test 17: Test a Missing Order
        [Fact]
        public async Task GetOrderByIdAsync_WhenOrderDoesNotExist_ReturnsNull()
        {
            // Arrange
            const int orderId = 999;

            _orderRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(orderId))
                .ReturnsAsync((Order?)null);

            // Act
            var result =
                await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            Assert.Null(result);

            _orderRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(orderId),
                Times.Once);
        }

        //Test 18: Test an Existing Order
        [Fact]
        public async Task GetOrderByIdAsync_WhenOrderExists_ReturnsMappedOrderResponse()
        {
            // Arrange
            const int orderId = 101;

            var customer = new Customer
            {
                Id = 1,
                Name = "Pranaya Rout",
                Email = "pranaya@example.com"
            };

            var laptop = new Product
            {
                Id = 1,
                Name = "Laptop",
                Price = 60000m,
                Stock = 18,
                Description = "High performance laptop"
            };

            var mouse = new Product
            {
                Id = 3,
                Name = "Wireless Mouse",
                Price = 1500m,
                Stock = 97,
                Description = "Ergonomic wireless mouse"
            };

            var orderDate =
                new DateTime(
                    2026,
                    7,
                    27,
                    10,
                    30,
                    0,
                    DateTimeKind.Utc);

            var order = new Order
            {
                Id = orderId,
                CustomerId = customer.Id,
                Customer = customer,
                OrderDate = orderDate,
                BaseAmount = 124500m,
                DiscountAmount = 6225m,
                TotalAmount = 118275m,
                OrderItems = new List<OrderItem>
                {
                    new()
                    {
                        Id = 1001,
                        OrderId = orderId,
                        ProductId = laptop.Id,
                        Product = laptop,
                        Quantity = 2,
                        UnitPrice = 60000m,
                        LineTotal = 120000m
                    },
                    new()
                    {
                        Id = 1002,
                        OrderId = orderId,
                        ProductId = mouse.Id,
                        Product = mouse,
                        Quantity = 3,
                        UnitPrice = 1500m,
                        LineTotal = 4500m
                    }
                }
            };

            _orderRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(orderId))
                .ReturnsAsync(order);

            // Act
            var result =
                await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(orderId, result.OrderId);
            Assert.Equal(customer.Id, result.CustomerId);
            Assert.Equal(customer.Name, result.CustomerName);
            Assert.Equal(customer.Email, result.CustomerEmail);
            Assert.Equal(orderDate, result.OrderDate);
            Assert.Equal(124500m, result.BaseAmount);
            Assert.Equal(6225m, result.DiscountAmount);
            Assert.Equal(118275m, result.TotalAmount);

            Assert.Equal(2, result.OrderItems.Count);

            var laptopItem =
                Assert.Single(
                    result.OrderItems,
                    item => item.ProductId == laptop.Id);

            Assert.Equal(1001, laptopItem.OrderItemId);
            Assert.Equal("Laptop", laptopItem.ProductName);
            Assert.Equal(
                "High performance laptop",
                laptopItem.Description);
            Assert.Equal(2, laptopItem.Quantity);
            Assert.Equal(60000m, laptopItem.UnitPrice);
            Assert.Equal(120000m, laptopItem.LineTotal);

            var mouseItem =
                Assert.Single(
                    result.OrderItems,
                    item => item.ProductId == mouse.Id);

            Assert.Equal(1002, mouseItem.OrderItemId);
            Assert.Equal(
                "Wireless Mouse",
                mouseItem.ProductName);
            Assert.Equal(
                "Ergonomic wireless mouse",
                mouseItem.Description);
            Assert.Equal(3, mouseItem.Quantity);
            Assert.Equal(1500m, mouseItem.UnitPrice);
            Assert.Equal(4500m, mouseItem.LineTotal);

            _orderRepositoryMock.Verify(
                repository =>
                    repository.GetByIdAsync(orderId),
                Times.Once);
        }




    }
}
