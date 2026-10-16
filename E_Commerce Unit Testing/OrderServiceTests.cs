using AutoMapper;
using Domain_Layer.Contracts;
using Domain_Layer.Exceptions;
using Domain_Layer.Models.BasketModule;
using Domain_Layer.Models.OrderModule;
using Domain_Layer.Models.ProductModule;
using Moq;
using Services;
using Shared.DTO.IdentityModule;
using Shared.DTO.OrderModule;

namespace E_Commerce_Unit_Testing
{
    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBasketRepository> _basketRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly OrderService _orderService;
        private const string email = "alaa@gmail.com";

        public OrderServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _basketRepoMock = new Mock<IBasketRepository>();
            _mapperMock = new Mock<IMapper>();

            _orderService = new OrderService(
                _mapperMock.Object,
                _basketRepoMock.Object,
                _unitOfWorkMock.Object
            );
        }

        #region CreateOrderAsync

        [Fact]
        public async Task CreateOrderAsync_BasketNotFound_ReturnBasketNotFoundException()
        {
            //Arrange
            var order = CreateOrderDto();
            //  Act
            Func<Task> Create = () => _orderService.CreateOrderAsync(order, email);

            // Assert

            await Assert.ThrowsAsync<BasketNotFoundException>(Create);
        }

        [Fact]
        public async Task CreateOrderAsync_ProductNotFound_ReturnProductNotFoundException()
        {
            //Arrange
            var basket = CreateBasket(new BasketItem { id = 99, Quantity = 2 });
            var order = CreateOrderDto();
            _basketRepoMock.Setup(B => B.GetBasketAsync(order.BasketId)).ReturnsAsync(basket);
            _unitOfWorkMock.Setup(uow => uow.GetRepository<Product, int>().GetByIdAsync(99)).ReturnsAsync((Product?)null);

            //  Act
            Func<Task> Create = () => _orderService.CreateOrderAsync(order, email);

            // Assert

            await Assert.ThrowsAsync<ProductNotFoundException>(Create);
        }

        [Fact]
        public async Task CreateOrderAsync_deliveryMethodNotFound_ReturnDeliveryMethodNotFoundException()
        {
            //Arrange
            var basket = CreateBasket(new BasketItem { id = 1, Quantity = 2 });
            var product = new Product { Id = 1, Name = "Product 1", Price = 10.0m, PictureUrl = "https://example.com/product1.jpg" };
            var order = CreateOrderDto();
            _basketRepoMock.Setup(B => B.GetBasketAsync(order.BasketId)).ReturnsAsync(basket);
            _unitOfWorkMock.Setup(uow => uow.GetRepository<Product, int>().GetByIdAsync(1)).ReturnsAsync(product);
            _unitOfWorkMock.Setup(uow => uow.GetRepository<DeliveryMethod, int>().GetByIdAsync(order.DeliveryMethodId)).ReturnsAsync((DeliveryMethod?)null);
            //  Act
            Func<Task> Create = () => _orderService.CreateOrderAsync(order, email);

            // Assert

            await Assert.ThrowsAsync<DeliveryMethodNotFoundException>(Create);
        }

        [Fact]
        public async Task CreateOrderAsync_OrderCreatedSuccessfully_ReturnOrderToReturnDto()
        {
            //Arrange

            #region Data
            var basket = CreateBasket(new BasketItem { id = 1, Quantity = 3 }, new BasketItem { id = 2, Quantity = 2 });
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Price = 300, PictureUrl = "https://example.com/product1.jpg" },
                new Product { Id = 2, Name = "Product 2", Price = 150, PictureUrl = "https://example.com/product1.jpg" }

            };
            var deliveryMethod = new DeliveryMethod { Id = 1, ShortName = "Fast", Description = "Fast Delivery", Price = 250.5m };
            var order = CreateOrderDto();

            #endregion

            #region Mapping Setup

            _mapperMock.Setup(m => m.Map<AddressDto, OrderAddress>(order.Address)).Returns(new OrderAddress());
            _mapperMock.Setup(m => m.Map<Order, OrderToReturnDto>(It.IsAny<Order>())).Returns((Order o) => new OrderToReturnDto { Total = o.GetTotal() });

            #endregion

            #region Repository Setup
            _basketRepoMock.Setup(B => B.GetBasketAsync(order.BasketId)).ReturnsAsync(basket);
            _unitOfWorkMock.Setup(uow => uow.GetRepository<Product, int>().GetByIdAsync(1)).ReturnsAsync(products[0]);
            _unitOfWorkMock.Setup(uow => uow.GetRepository<Product, int>().GetByIdAsync(2)).ReturnsAsync(products[1]);
            _unitOfWorkMock.Setup(uow => uow.GetRepository<DeliveryMethod, int>().GetByIdAsync(order.DeliveryMethodId)).ReturnsAsync(deliveryMethod);
            _unitOfWorkMock.Setup(uow => uow.GetRepository<Order, Guid>().AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            #endregion

            //  Act
            var r = await _orderService.CreateOrderAsync(order, email);
            var actual = r.Total;
            var expected = 1450.5m;
            // Assert

            Assert.Equal(expected, actual);
        }

        #region Private Helper Methods

        private static OrderDto CreateOrderDto(string basketId = "b2", int deliveryMethodId = 1)
        {
            return new OrderDto
            {
                BasketId = basketId,
                DeliveryMethodId = deliveryMethodId,
                Address = new AddressDto
                {
                    FirstName = "Alaa",
                    LastName = "Ibrahim",
                    Street = "123 Main St",
                    City = "Fayoum",
                    Country = "Egypt"
                }
            };
        }

        private static Basket CreateBasket(params BasketItem[] items)
        {
            return new Basket
            {
                Id = "b2",
                Items = items.ToList()
            };
        }

        #endregion

        #endregion

    }
}