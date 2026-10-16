using AutoMapper;
using Domain_Layer.Exceptions;
using Domain_Layer.Models.IdentityModule;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using Moq;
using Shared.DTO.IdentityModule;
using AuthenticationService = Services.AuthenticationService;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
namespace E_Commerce_Unit_Testing
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly AuthenticationService _authService;

        public AuthenticationServiceTests()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(new Mock<IUserStore<ApplicationUser>>().Object, null, null, null, null, null, null, null, null);
            _mapperMock = new Mock<IMapper>();
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c.GetSection("JWTOptions")["SecretKey"]).Returns("SuperSecretKeyForTestingJwtTokens12345!");
            _configurationMock.Setup(c => c.GetSection("JWTOptions")["Issuer"]).Returns("http://localhost");
            _configurationMock.Setup(c => c.GetSection("JWTOptions")["Audience"]).Returns("http://localhost");
            _authService = new AuthenticationService(
                _userManagerMock.Object,
                _configurationMock.Object,
                _mapperMock.Object
            );
        }

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_WhenUserDoesNotExist_ThrowsUserNotFoundException()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "notfound@gmail.com", Password = "Password123!" };
            _userManagerMock.Setup(m => m.FindByEmailAsync(loginDto.Email)).ReturnsAsync((ApplicationUser?)null);

            // Act 
            Func<Task> act = () => _authService.LoginAsync(loginDto);
            // Assert

            await Assert.ThrowsAsync<UserNotFoundException>(act);
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordIsIncorrect_ThrowsUnAuthorizedException()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "alaa@gmail.com", Password = "WrongPassword" };
            var user = new ApplicationUser { Id = "1", Email = loginDto.Email, UserName = "alaa", DisplayName = "Alaa" };

            _userManagerMock.Setup(m => m.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(false);

            // Act
            Func<Task> act = () => _authService.LoginAsync(loginDto);
            // Assert
            await Assert.ThrowsAsync<UnAuthorizedException>(act);
        }

        [Fact]
        public async Task LoginAsync_WhenCredentialsAreValid_ReturnsUserDtoWithToken()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "alaa@gmail.com", Password = "CorrectPassword123!" };
            var user = new ApplicationUser { Id = "1", Email = loginDto.Email, UserName = "alaa", DisplayName = "Alaa" };

            _userManagerMock.Setup(m => m.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);
            _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(loginDto.Email, result.Email);
            Assert.Equal(user.DisplayName, result.DisplayName);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }

        #endregion

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_WhenCreationFailed_ThrowsBadRequestException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "alaa@gmail.com",
                Password = "weak",
                DisplayName = "Alaa",
                UserName = "alaa123",
                PhoneNumber = "01000000000"
            };
            var identityErrors = new IdentityError[] { new IdentityError { Description = "Password requires at least 6 characters" } };

            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password)).ReturnsAsync(IdentityResult.Failed(identityErrors));

            // Act
            Func<Task> act = () => _authService.RegisterAsync(registerDto);
            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task RegisterAsync_WhenSuccessful_ReturnsUserDtoWithToken()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "newuser@gmail.com",
                Password = "Password123!",
                DisplayName = "Alaa",
                UserName = "alaa123",
                PhoneNumber = "01000000000"
            };

            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(registerDto.Email, result.Email);
            Assert.Equal(registerDto.DisplayName, result.DisplayName);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }

        #endregion

        #region GetCurrentUserAsync Tests

        [Fact]
        public async Task GetCurrentUserAsync_WhenUserNotFound_ThrowsUserNotFoundException()
        {
            // Arrange
            var email = "alaa5@gmail.com";
            _userManagerMock.Setup(m => m.FindByEmailAsync(email)).ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = () => _authService.GetCurrentUserAsync(email);

            // Assert
            await Assert.ThrowsAsync<UserNotFoundException>(act);
        }

        [Fact]
        public async Task GetCurrentUserAsync_WhenUserExists_ReturnsUserDtoWithToken()
        {
            // Arrange
            var email = "alaa@gmail.com";
            var user = new ApplicationUser { Id = "1", Email = email, UserName = "alaa", DisplayName = "Alaa" };

            _userManagerMock.Setup(m => m.FindByEmailAsync(email)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _authService.GetCurrentUserAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
            Assert.Equal(user.DisplayName, result.DisplayName);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }

        #endregion

        #region CheckEmailAsync Tests

        [Fact]
        public async Task CheckEmailAsync_WhenUserExists_ReturnsTrue()
        {
            // Arrange
            var email = "existing@gmail.com";
            var user = new ApplicationUser { Email = email };

            _userManagerMock.Setup(m => m.FindByEmailAsync(email)).ReturnsAsync(user);

            // Act
            var result = await _authService.CheckEmailAsync(email);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckEmailAsync_WhenUserDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var email = "notfound@gmail.com";

            _userManagerMock.Setup(m => m.FindByEmailAsync(email)).ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authService.CheckEmailAsync(email);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetCurrentUserAddressAsync Tests

        [Fact]
        public async Task GetCurrentUserAddressAsync_WhenUserNotFound_ThrowsUserNotFoundException()
        {
            // Arrange
            var email = "alaa@gmail.com";
            var emptyUsersList = new List<ApplicationUser>().BuildMock();

            _userManagerMock.Setup(m => m.Users).Returns(emptyUsersList);

            // Act
            Func<Task> act = () => _authService.GetCurrentUserAddressAsync(email);
            // Assert
            await Assert.ThrowsAsync<UserNotFoundException>(act);
        }

        [Fact]
        public async Task GetCurrentUserAddressAsync_WhenAddressIsNull_ThrowsAddressNotFoundException()
        {
            // Arrange
            var email = "alaa@gmail.com";
            var user = new ApplicationUser
            {
                Email = email,
                UserName = "alaa123",
                Address = null
            };

            var usersList = new List<ApplicationUser> { user }.BuildMock();
            _userManagerMock.Setup(m => m.Users).Returns(usersList);

            // Act
            Func<Task> act = () => _authService.GetCurrentUserAddressAsync(email);

            // Assert
            await Assert.ThrowsAsync<AddressNotFoundException>(act);
        }

        [Fact]
        public async Task GetCurrentUserAddressAsync_WhenUserAndAddressExist_ReturnsAddressDto()
        {
            // Arrange
            var email = "alaa@gmail.com";
            var address = new Address { FirstName = "Alaa", City = "Cairo", Country = "Egypt" };
            var addressDto = new AddressDto { FirstName = "Alaa", City = "Cairo", Country = "Egypt" };
            var user = new ApplicationUser { Email = email, UserName = "alaa123", Address = address };

            var usersList = new List<ApplicationUser> { user }.BuildMock();
            _userManagerMock.Setup(m => m.Users).Returns(usersList);
            _mapperMock.Setup(m => m.Map<Address, AddressDto>(address)).Returns(addressDto);

            // Act
            var result = await _authService.GetCurrentUserAddressAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Cairo", result.City);
        }

        #endregion

        #region UpdateCurrentUserAddressAsync Tests

        [Fact]
        public async Task UpdateCurrentUserAddressAsync_WhenUserNotFound_ThrowsUserNotFoundException()
        {
            // Arrange
            var email = "notfound@gmail.com";
            var addressDto = new AddressDto { FirstName = "NewName" };
            var emptyUsersList = new List<ApplicationUser>().BuildMock();

            _userManagerMock.Setup(m => m.Users).Returns(emptyUsersList);

            // Act
            Func<Task> act = () => _authService.UpdateCurrentUserAddressAsync(email, addressDto);
            // Assert
            await Assert.ThrowsAsync<UserNotFoundException>(act);
        }

        [Fact]
        public async Task UpdateCurrentUserAddressAsync_WhenAddressAlreadyExists_UpdatesExistingAddress()
        {
            // Arrange
            var email = "alaa@gmail.com";
            var address = new Address { FirstName = "OldName", City = "Alex" };
            var user = new ApplicationUser { Email = email, Address = address };

            var addressDto = new AddressDto { FirstName = "NewName", LastName = "Ali", Street = "Street 1", City = "Cairo", Country = "Egypt" };

            var usersList = new List<ApplicationUser> { user }.BuildMock();
            _userManagerMock.Setup(m => m.Users).Returns(usersList);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _mapperMock.Setup(m => m.Map<Address, AddressDto>(address)).Returns(addressDto);

            // Act
            var result = await _authService.UpdateCurrentUserAddressAsync(email, addressDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addressDto, result);
        }

        [Fact]
        public async Task UpdateCurrentUserAddressAsync_WhenAddressIsNull_CreatesNewAddress()
        {
            // Arrange
            var email = "alaa@gmail.com";
            var user = new ApplicationUser { Email = email, Address = null };

            var addressDto = new AddressDto { FirstName = "NewUser", City = "Giza" };
            var address = new Address { FirstName = "NewUser", City = "Giza" };

            var usersList = new List<ApplicationUser> { user }.BuildMock();
            _userManagerMock.Setup(m => m.Users).Returns(usersList);
            _mapperMock.Setup(m => m.Map<AddressDto, Address>(addressDto)).Returns(address);
            _mapperMock.Setup(m => m.Map<Address, AddressDto>(address)).Returns(addressDto);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.UpdateCurrentUserAddressAsync(email, addressDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(user.Address);
            Assert.Equal("Giza", result.City);
        }

        #endregion
    }
}
