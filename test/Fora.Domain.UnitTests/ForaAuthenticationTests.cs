using Fora.Domain.Entities;
using Fora.Domain.Interfaces.Infra.Data;
using Fora.Domain.Services;
using Fora.Infra.Security.Interfaces;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fora.Domain.UnitTests
{
    public class ForaAuthenticationServiceTests
    {
        private readonly IWebUserRepository _webUserRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ForaAuthenticationService _sut;

        public ForaAuthenticationServiceTests()
        {
            _webUserRepository = Substitute.For<IWebUserRepository>();
            _passwordHasher = Substitute.For<IPasswordHasher>();
            _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
            _sut = new ForaAuthenticationService(_webUserRepository, _passwordHasher, _jwtTokenGenerator);
        }

        #region AuthenticateUserForLogin - Success Tests

        [Fact]
        public async Task AuthenticateUserForLogin_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var userName = "testuser";
            var password = "password123";
            var hashedPassword = "hashed_password_123";
            var expectedToken = "jwt_token_here";
            var webUser = new WebUser
            {
                UserName = userName,
                Role = "Admin",
                Password = hashedPassword
            };

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword).Returns(webUser);
            _jwtTokenGenerator.GenerateForLogin(userName, "Admin").Returns(expectedToken);

            // Act
            var result = await _sut.AuthenticateUserForLogin(userName, password);

            // Assert
            Assert.Equal(expectedToken, result);
        }

        [Fact]
        public async Task AuthenticateUserForLogin_WithValidCredentials_ShouldHashPassword()
        {
            // Arrange
            var userName = "testuser";
            var password = "password123";
            var hashedPassword = "hashed_password_123";
            var webUser = new WebUser { UserName = userName, Role = "User", Password = hashedPassword };

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword).Returns(webUser);
            _jwtTokenGenerator.GenerateForLogin(userName, "User").Returns("token");

            // Act
            await _sut.AuthenticateUserForLogin(userName, password);

            // Assert
            _passwordHasher.Received(1).HashPasswordWithKey(password);
        }

        [Fact]
        public async Task AuthenticateUserForLogin_WithValidCredentials_ShouldCallRepositoryWithHashedPassword()
        {
            // Arrange
            var userName = "testuser";
            var password = "password123";
            var hashedPassword = "hashed_password_123";
            var webUser = new WebUser { UserName = userName, Role = "User", Password = hashedPassword };

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword).Returns(webUser);
            _jwtTokenGenerator.GenerateForLogin(userName, "User").Returns("token");

            // Act
            await _sut.AuthenticateUserForLogin(userName, password);

            // Assert
            await _webUserRepository.Received(1).GetForLogin(userName, hashedPassword);
        }

        [Fact]
        public async Task AuthenticateUserForLogin_WithValidCredentials_ShouldGenerateTokenWithCorrectUserNameAndRole()
        {
            // Arrange
            var userName = "testuser";
            var password = "password123";
            var hashedPassword = "hashed_password_123";
            var role = "SuperAdmin";
            var webUser = new WebUser { UserName = userName, Role = role, Password = hashedPassword };

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword).Returns(webUser);
            _jwtTokenGenerator.GenerateForLogin(userName, role).Returns("token");

            // Act
            await _sut.AuthenticateUserForLogin(userName, password);

            // Assert
            _jwtTokenGenerator.Received(1).GenerateForLogin(userName, role);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("User")]
        [InlineData("Guest")]
        [InlineData("SuperAdmin")]
        public async Task AuthenticateUserForLogin_WithDifferentRoles_ShouldGenerateTokenWithCorrectRole(string role)
        {
            // Arrange
            var userName = "testuser";
            var password = "password123";
            var hashedPassword = "hashed_password_123";
            var expectedToken = $"token_for_{role}";
            var webUser = new WebUser { UserName = userName, Role = role, Password = hashedPassword };

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword).Returns(webUser);
            _jwtTokenGenerator.GenerateForLogin(userName, role).Returns(expectedToken);

            // Act
            var result = await _sut.AuthenticateUserForLogin(userName, password);

            // Assert
            Assert.Equal(expectedToken, result);
            _jwtTokenGenerator.Received(1).GenerateForLogin(userName, role);
        }

        #endregion

        #region AuthenticateUserForLogin - Failure Tests

        [Fact]
        public async Task AuthenticateUserForLogin_WithInvalidUsername_ShouldThrowException()
        {
            // Arrange
            var userName = "invaliduser";
            var password = "password123";
            var hashedPassword = "hashed_password_123";

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword).Returns((WebUser?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.AuthenticateUserForLogin(userName, password));

            Assert.Equal("User or Password is wrong", exception.Message);
        }

        [Fact]
        public async Task AuthenticateUserForLogin_WithInvalidPassword_ShouldThrowException()
        {
            // Arrange
            var userName = "testuser";
            var password = "wrongpassword";
            var hashedPassword = "wrong_hashed_password";

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword).Returns((WebUser?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.AuthenticateUserForLogin(userName, password));

            Assert.Equal("User or Password is wrong", exception.Message);
        }

        [Fact]
        public async Task AuthenticateUserForLogin_WhenUserNotFound_ShouldNotGenerateToken()
        {
            // Arrange
            var userName = "testuser";
            var password = "password123";
            var hashedPassword = "hashed_password_123";

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword).Returns((WebUser?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                async () => await _sut.AuthenticateUserForLogin(userName, password));

            _jwtTokenGenerator.DidNotReceive().GenerateForLogin(Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AuthenticateUserForLogin_WithEmptyOrWhitespaceUsername_ShouldStillProcessAndFail(string userName)
        {
            // Arrange
            var password = "password123";
            var hashedPassword = "hashed_password_123";

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword).Returns((WebUser?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.AuthenticateUserForLogin(userName, password));

            Assert.Equal("User or Password is wrong", exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AuthenticateUserForLogin_WithEmptyOrWhitespacePassword_ShouldStillHashAndProcess(string password)
        {
            // Arrange
            var userName = "testuser";
            var hashedPassword = "hashed_empty_password";

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword).Returns((WebUser?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                async () => await _sut.AuthenticateUserForLogin(userName, password));

            _passwordHasher.Received(1).HashPasswordWithKey(password);
        }

        #endregion

        #region AuthenticateUserForLogin - Edge Cases

        [Fact]
        public async Task AuthenticateUserForLogin_WithSpecialCharactersInUsername_ShouldProcess()
        {
            // Arrange
            var userName = "user@test.com";
            var password = "password123";
            var hashedPassword = "hashed_password_123";
            var webUser = new WebUser { UserName = userName, Role = "User", Password = hashedPassword };

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword).Returns(webUser);
            _jwtTokenGenerator.GenerateForLogin(userName, "User").Returns("token");

            // Act
            var result = await _sut.AuthenticateUserForLogin(userName, password);

            // Assert
            Assert.NotNull(result);
            await _webUserRepository.Received(1).GetForLogin(userName, hashedPassword);
        }

        [Fact]
        public async Task AuthenticateUserForLogin_WithLongPassword_ShouldProcess()
        {
            // Arrange
            var userName = "testuser";
            var password = new string('a', 1000); // Very long password
            var hashedPassword = "hashed_long_password";
            var webUser = new WebUser { UserName = userName, Role = "User", Password = hashedPassword };

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword).Returns(webUser);
            _jwtTokenGenerator.GenerateForLogin(userName, "User").Returns("token");

            // Act
            var result = await _sut.AuthenticateUserForLogin(userName, password);

            // Assert
            Assert.NotNull(result);
            _passwordHasher.Received(1).HashPasswordWithKey(password);
        }

        [Fact]
        public async Task AuthenticateUserForLogin_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var userName = "testuser";
            var password = "password123";
            var hashedPassword = "hashed_password_123";

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword)
                .Throws(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sut.AuthenticateUserForLogin(userName, password));
        }

        [Fact]
        public async Task AuthenticateUserForLogin_WhenHasherThrowsException_ShouldPropagateException()
        {
            // Arrange
            var userName = "testuser";
            var password = "password123";

            _passwordHasher.HashPasswordWithKey(password).Throws(new ArgumentException("Hashing error"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _sut.AuthenticateUserForLogin(userName, password));
        }

        [Fact]
        public async Task AuthenticateUserForLogin_WhenTokenGeneratorThrowsException_ShouldPropagateException()
        {
            // Arrange
            var userName = "testuser";
            var password = "password123";
            var hashedPassword = "hashed_password_123";
            var webUser = new WebUser { UserName = userName, Role = "User", Password = hashedPassword };

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword).Returns(webUser);
            _jwtTokenGenerator.GenerateForLogin(userName, "User")
                .Throws(new InvalidOperationException("Token generation error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sut.AuthenticateUserForLogin(userName, password));
        }

        #endregion

        #region Integration Flow Tests

        [Fact]
        public async Task AuthenticateUserForLogin_ShouldCallDependenciesInCorrectOrder()
        {
            // Arrange
            var userName = "testuser";
            var password = "password123";
            var hashedPassword = "hashed_password_123";
            var webUser = new WebUser { UserName = userName, Role = "User", Password = hashedPassword };
            var callOrder = new List<string>();

            _passwordHasher.HashPasswordWithKey(password).Returns(x =>
            {
                callOrder.Add("HashPassword");
                return hashedPassword;
            });

            _webUserRepository.GetForLogin(userName, hashedPassword).Returns(x =>
            {
                callOrder.Add("GetUser");
                return Task.FromResult<WebUser?>(webUser);
            });

            _jwtTokenGenerator.GenerateForLogin(userName, "User").Returns(x =>
            {
                callOrder.Add("GenerateToken");
                return "token";
            });

            // Act
            await _sut.AuthenticateUserForLogin(userName, password);

            // Assert
            Assert.Equal(3, callOrder.Count);
            Assert.Equal("HashPassword", callOrder[0]);
            Assert.Equal("GetUser", callOrder[1]);
            Assert.Equal("GenerateToken", callOrder[2]);
        }

        [Fact]
        public async Task AuthenticateUserForLogin_WhenUserNotFound_ShouldStopAtRepositoryCall()
        {
            // Arrange
            var userName = "testuser";
            var password = "password123";
            var hashedPassword = "hashed_password_123";

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword).Returns((WebUser?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                async () => await _sut.AuthenticateUserForLogin(userName, password));

            _passwordHasher.Received(1).HashPasswordWithKey(password);
            await _webUserRepository.Received(1).GetForLogin(userName, hashedPassword);
            _jwtTokenGenerator.DidNotReceive().GenerateForLogin(Arg.Any<string>(), Arg.Any<string>());
        }

        #endregion

        #region Case Sensitivity Tests

        [Fact]
        public async Task AuthenticateUserForLogin_WithDifferentCaseUsername_ShouldProcessAsIs()
        {
            // Arrange
            var userName = "TestUser"; // Mixed case
            var password = "password123";
            var hashedPassword = "hashed_password_123";
            var webUser = new WebUser { UserName = userName, Role = "User", Password = hashedPassword };

            _passwordHasher.HashPasswordWithKey(password).Returns(hashedPassword);
            _webUserRepository.GetForLogin(userName, hashedPassword).Returns(webUser);
            _jwtTokenGenerator.GenerateForLogin(userName, "User").Returns("token");

            // Act
            var result = await _sut.AuthenticateUserForLogin(userName, password);

            // Assert
            Assert.NotNull(result);
            await _webUserRepository.Received(1).GetForLogin(userName, hashedPassword);
            _jwtTokenGenerator.Received(1).GenerateForLogin(userName, "User");
        }

        #endregion
    }
}