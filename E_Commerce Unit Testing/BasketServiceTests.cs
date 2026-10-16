using AutoMapper;
using Domain_Layer.Contracts;
using Domain_Layer.Exceptions;
using Domain_Layer.Models.BasketModule;
using Moq;
using Services;
using Shared.DTO.BasketModule;

namespace E_Commerce_Unit_Testing
{
    public class BasketServiceTests
    {
        private readonly Mock<IBasketRepository> _basketRepoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly BasketService _basketService;

        private const string BasketId = "b123";

        public BasketServiceTests()
        {
            _basketService = new BasketService(_basketRepoMock.Object, _mapperMock.Object);
        }

        #region GetBasketAsync Tests

        [Fact]
        public async Task GetBasketAsync_BasketNotFound_ReturnBasketNotFoundException()
        {
            // Arrange
            _basketRepoMock.Setup(r => r.GetBasketAsync(BasketId)).ReturnsAsync((Basket?)null);

            // Act
            Func<Task> action = () => _basketService.GetBasketAsync(BasketId);
            // Assert
            await Assert.ThrowsAsync<BasketNotFoundException>(action);
        }

        [Fact]
        public async Task GetBasketAsync_BasketIsFound_ReturnsBasketDto()
        {
            // Arrange
            var basket = new Basket { Id = BasketId };
            var expectedDto = new BasketDto { Id = BasketId };

            _basketRepoMock.Setup(r => r.GetBasketAsync(BasketId)).ReturnsAsync(basket);
            _mapperMock.Setup(m => m.Map<Basket, BasketDto>(basket)).Returns(expectedDto);

            // Act
            var result = await _basketService.GetBasketAsync(BasketId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(BasketId, result.Id);
        }

        #endregion

        #region CreateOrUpdateBasketAsync Tests

        [Fact]
        public async Task CreateOrUpdateBasketAsync_WhenProcessFails_ReturnException()
        {
            // Arrange
            var basketDto = new BasketDto { Id = BasketId };
            var basket = new Basket { Id = BasketId };

            _mapperMock.Setup(m => m.Map<BasketDto, Basket>(basketDto)).Returns(basket);
            _basketRepoMock.Setup(r => r.CreateOrUpdateBasketAsync(basket, It.IsAny<TimeSpan?>())).ReturnsAsync((Basket?)null);

            // Act
            Func<Task> action = () => _basketService.CreateOrUpdateBasketAsync(basketDto);
            // Assert
            await Assert.ThrowsAsync<Exception>(action);
        }

        [Fact]
        public async Task CreateOrUpdateBasketAsync_WhenSuccessful_ReturnsUpdatedBasketDto()
        {
            // Arrange
            var basketDto = new BasketDto { Id = BasketId };
            var basket = new Basket { Id = BasketId };

            _mapperMock.Setup(m => m.Map<BasketDto, Basket>(basketDto)).Returns(basket);
            _mapperMock.Setup(m => m.Map<Basket, BasketDto>(basket)).Returns(basketDto);
            _basketRepoMock.Setup(r => r.CreateOrUpdateBasketAsync(basket, It.IsAny<TimeSpan?>())).ReturnsAsync(basket);
            _basketRepoMock.Setup(r => r.GetBasketAsync(BasketId)).ReturnsAsync(basket);

            // Act
            var result = await _basketService.CreateOrUpdateBasketAsync(basketDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(BasketId, result.Id);
        }

        #endregion
    }
}
