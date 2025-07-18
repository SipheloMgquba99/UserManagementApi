using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UserManagement.Application.Contracts;
using UserManagement.Application.Dtos;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Tests.Builders;
using UserManagement.Tests.Builders.Repository;
using FVValidationResult = FluentValidation.Results.ValidationResult;

namespace UserManagement.Tests.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IAuthRepository> _repoMock;
        private readonly Mock<IValidator<RegisterUserDto>> _registerValidator;
        private readonly Mock<IValidator<LoginUserDto>> _loginValidator;
        private readonly Mock<IValidator<string>> _passwordValidator;
        private readonly Mock<IConfiguration> _config;
        private readonly Mock<ILogger<AuthService>> _logger;
        private readonly AuthRepositoryBuilder _userBuilder;
        private readonly RegisterUserDtoBuilder _registerUserDtoBuilder;
        private readonly LoginUserDtoBuilder _loginUserDtoBuilder;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _repoMock = new Mock<IAuthRepository>();
            _registerValidator = new Mock<IValidator<RegisterUserDto>>();
            _loginValidator = new Mock<IValidator<LoginUserDto>>();
            _passwordValidator = new Mock<IValidator<string>>();
            _config = new Mock<IConfiguration>();
            _logger = new Mock<ILogger<AuthService>>();
            _userBuilder = new AuthRepositoryBuilder();
            _registerUserDtoBuilder = new RegisterUserDtoBuilder();
            _loginUserDtoBuilder = new LoginUserDtoBuilder();

            _config.Setup(x => x["Jwt:Key"]).Returns("YourVeryLongSecretKeyThatIsAtLeast32Chars!");
            _config.Setup(x => x["Jwt:Issuer"]).Returns("https://localhost");
            _config.Setup(x => x["Jwt:Audience"]).Returns("https://localhost");

            _authService = new AuthService(
                _repoMock.Object,
                _config.Object,
                _registerValidator.Object,
                _loginValidator.Object,
                _passwordValidator.Object,
                _logger.Object);
        }

        [Fact]
        public async Task RegisterUserAsync_Should_Return_Success()
        {
            var dto = _registerUserDtoBuilder.Build();

            _registerValidator.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new FVValidationResult());
            _repoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null);
            _repoMock.Setup(r => r.AddUserAsync(It.IsAny<ApplicationUser>())).Returns(Task.CompletedTask);

            var result = await _authService.RegisterUserAsync(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal("Registration successful", result.Data?.Message);
        }

        [Fact]
        public async Task RegisterUserAsync_Should_Fail_When_UserExists()
        {
            var dto = _registerUserDtoBuilder.WithEmail("existing@example.com").Build();

            _registerValidator.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new FVValidationResult());
            _repoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync(new ApplicationUser());

            var result = await _authService.RegisterUserAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal("User already exists.", result.Message);
        }

        [Fact]
        public async Task LoginUserAsync_Should_Return_Token()
        {
            var user = _userBuilder.Build();
            var dto = _loginUserDtoBuilder
                .WithEmail(user.Email)
                .WithPassword(user.Password)
                .Build();

            _loginValidator.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new FVValidationResult());
            _repoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync(user);

            var result = await _authService.LoginUserAsync(dto);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data?.Token);
        }

        [Fact]
        public async Task ChangePasswordAsync_Should_Succeed()
        {
            var user = _userBuilder.Build();
            _repoMock.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
            _passwordValidator.Setup(p => p.ValidateAsync("NewPass123#", default))
                .ReturnsAsync(new FVValidationResult());
            _repoMock.Setup(r => r.UpdateUserAsync(It.IsAny<ApplicationUser>())).Returns(Task.CompletedTask);

            var result = await _authService.ChangePasswordAsync(user.Email, user.Password, "NewPass123#");

            Assert.True(result.IsSuccess);
            Assert.Equal("Password changed successfully.", result.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_Should_Fail_WrongOldPassword()
        {
            var user = _userBuilder.Build();
            _repoMock.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);

            var result = await _authService.ChangePasswordAsync(user.Email, "wrong", "NewPass123#");

            Assert.False(result.IsSuccess);
            Assert.Equal("Incorrect old password.", result.Message);
        }

        [Fact]
        public async Task ResetPasswordAsync_Should_Succeed()
        {
            var user = _userBuilder.Build();
            _repoMock.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
            _passwordValidator.Setup(p => p.ValidateAsync("Reset1234#", default))
                .ReturnsAsync(new FVValidationResult());
            _repoMock.Setup(r => r.UpdateUserAsync(It.IsAny<ApplicationUser>())).Returns(Task.CompletedTask);

            var result = await _authService.ResetPasswordAsync(user.Email, "Reset1234#");

            Assert.True(result.IsSuccess);
            Assert.Equal("Password has been reset.", result.Message);
        }

        [Fact]
        public async Task SetupProfileAsync_Should_Succeed()
        {
            var user = _userBuilder.Build();
            _repoMock.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
            _repoMock.Setup(r => r.UpdateUserAsync(It.IsAny<ApplicationUser>())).Returns(Task.CompletedTask);

            var result = await _authService.SetupProfileAsync(user.Email, "Jane", "Smith");

            Assert.True(result.IsSuccess);
            Assert.Equal("Profile updated successfully.", result.Message);
        }

        [Fact]
        public async Task LogoutAsync_Should_Return_Success()
        {
            var result = await _authService.LogoutAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal("Logout successful.", result.Message);
        }
    }
}
