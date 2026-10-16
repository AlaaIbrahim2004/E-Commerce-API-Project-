using AutoMapper;
using Domain_Layer.Contracts;
using Domain_Layer.Exceptions;
using Domain_Layer.Models.ProductModule;
using Moq;
using Services;
using Shared.DTO.ProductModule;

namespace E_Commerce_Unit_Testing
{
    public class ProductServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ProductService _productService;

        public ProductServiceTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _productService = new ProductService(
                _unitOfWorkMock.Object,
                _mapperMock.Object
            );
        }

        #region GetProductByIdAsync

        [Fact]

        public async Task GetProductByIdAsync_ProductNotFound_ReturnProductNotFoundException()
        {
            //Arrange
            var id = 1;
            _unitOfWorkMock.Setup(uow => uow.GetRepository<Product, int>().GetByIdAsync(It.IsAny<ISpecification<Product, int>>())).ReturnsAsync((Product?)null);
            //Act
            Func<Task> action = () => _productService.GetProductByIdAsync(id);
            //Assert
            await Assert.ThrowsAsync<ProductNotFoundException>(action);
        }

        [Fact]

        public async Task GetProductByIdAsync_ProductIsFound_ReturnProduct()
        {
            //Arrange
            var id = 1;
            var product = new Product { Id = id, Name = "Test Product" };
            var expectedDto = new ProductDto { Id = id, Name = "Test Product" };
            _unitOfWorkMock.Setup(uow => uow.GetRepository<Product, int>().GetByIdAsync(It.IsAny<ISpecification<Product, int>>())).ReturnsAsync((product));
            _mapperMock.Setup(m => m.Map<Product, ProductDto>(product)).Returns(expectedDto);
            var result = await _productService.GetProductByIdAsync(id);
            //Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal(expectedDto.Name, result.Name);
        }
    }
    #endregion

}
