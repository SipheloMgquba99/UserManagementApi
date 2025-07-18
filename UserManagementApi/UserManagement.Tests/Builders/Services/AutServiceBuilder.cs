using Bogus;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UserManagement.Application.Contracts;
using UserManagement.Application.Dtos;
using UserManagement.Application.Services;

namespace UserManagement.Tests.Builders.Services;

public class AuthServiceBuilder
{
    private readonly Mock<IAuthRepository> _authRepoMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<IValidator<RegisterUserDto>> _registerValidatorMock = new();
    private readonly Mock<IValidator<LoginUserDto>> _loginValidatorMock = new();
    private readonly Mock<IValidator<string>> _passwordValidatorMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();
    private readonly Faker _faker = new();

    public AuthServiceBuilder WithValidRegisterValidation()
    {
        _registerValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<RegisterUserDto>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        return this;
    }

    public AuthServiceBuilder WithValidLoginValidation()
    {
        _loginValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<LoginUserDto>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        return this;
    }

    public AuthServiceBuilder WithValidPasswordValidation()
    {
        _passwordValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        return this;
    }

    public AuthServiceBuilder WithUserExists(string email)
    {
        _authRepoMock
            .Setup(r => r.GetUserByEmailAsync(email))
            .ReturnsAsync(new Domain.Entities.ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                Password = _faker.Internet.Password(12, true, "[A-Z]", "#1a"),
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName(),
                Role = Domain.Entities.Roles.User
            });

        return this;
    }

    public AuthServiceBuilder WithUserNotFound(string email)
    {
        _authRepoMock
            .Setup(r => r.GetUserByEmailAsync(email))
            .ReturnsAsync((Domain.Entities.ApplicationUser?)null);

        return this;
    }

    public AuthServiceBuilder WithJwtConfig()
    {
        _configMock.Setup(c => c["Jwt:Key"]).Returns("ThisIsASecretKey12345!");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

        return this;
    }

    public AuthService Build()
    {
        return new AuthService(
            _authRepoMock.Object,
            _configMock.Object,
            _registerValidatorMock.Object,
            _loginValidatorMock.Object,
            _passwordValidatorMock.Object,
            _loggerMock.Object
        );
    }
}
