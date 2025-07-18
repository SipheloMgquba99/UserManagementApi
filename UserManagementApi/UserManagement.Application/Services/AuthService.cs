using FluentValidation;
using FluentValidationResult = FluentValidation.Results.ValidationResult;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagement.Application.Common;
using UserManagement.Application.Contracts;
using UserManagement.Application.Dtos;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IConfiguration _configuration;
    private readonly IValidator<RegisterUserDto> _registerValidator;
    private readonly IValidator<LoginUserDto> _loginValidator;
    private readonly IValidator<string> _passwordValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAuthRepository authRepository,
        IConfiguration configuration,
        IValidator<RegisterUserDto> registerValidator,
        IValidator<LoginUserDto> loginValidator,
        IValidator<string> passwordValidator,
        ILogger<AuthService> logger)
    {
        _authRepository = authRepository;
        _configuration = configuration;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _passwordValidator = passwordValidator;
        _logger = logger;
    }

    public async Task<bool> UserExistsAsync(string email) =>
        await _authRepository.UserExistsAsync(email);

    public async Task DeleteUserAsync(Guid userId) =>
        await _authRepository.DeleteUserAsync(userId);

    public async Task<ServiceResult<RegistrationResponseDto>> RegisterUserAsync(RegisterUserDto dto)
    {
        var validation = await _registerValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            var error = string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("User registration failed validation: {Errors}", error);
            return ServiceResult<RegistrationResponseDto>.Failure(error);
        }

        if (dto.Password != dto.ConfirmPassword)
        {
            _logger.LogWarning("User registration failed: Passwords do not match for email {Email}", dto.Email);
            return ServiceResult<RegistrationResponseDto>.Failure("Passwords do not match.");
        }

        var exists = await _authRepository.GetUserByEmailAsync(dto.Email);
        if (exists != null)
        {
            _logger.LogWarning("User registration failed: Email {Email} already exists", dto.Email);
            return ServiceResult<RegistrationResponseDto>.Failure("User already exists.");
        }

        try
        {
            var user = new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = dto.Password,
                Role = Roles.User
            };

            await _authRepository.AddUserAsync(user);
            return ServiceResult<RegistrationResponseDto>.Success(new RegistrationResponseDto(true, "Registration successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while registering user with email {Email}", dto.Email);
            return ServiceResult<RegistrationResponseDto>.Failure("Internal server error.");
        }
    }

    public async Task<ServiceResult<LoginResponseDto>> LoginUserAsync(LoginUserDto dto)
    {
        var validation = await _loginValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            var error = string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Login failed validation for email {Email}: {Errors}", dto.Email, error);
            return ServiceResult<LoginResponseDto>.Failure(error);
        }

        var user = await _authRepository.GetUserByEmailAsync(dto.Email);
        if (user == null || user.Password != dto.Password)
        {
            _logger.LogWarning("Login failed: Invalid credentials for email {Email}", dto.Email);
            return ServiceResult<LoginResponseDto>.Failure("Invalid email or password.");
        }

        try
        {
            string token = GenerateJWTToken(user);
            return ServiceResult<LoginResponseDto>.Success(new LoginResponseDto(true, "Login successful", token));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JWT generation failed for user {Email}", dto.Email);
            return ServiceResult<LoginResponseDto>.Failure("Internal server error.");
        }
    }

    public async Task<ServiceResult> SetupProfileAsync(string email, string firstName, string lastName)
    {
        var user = await _authRepository.GetUserByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("Profile update failed: User not found for email {Email}", email);
            return ServiceResult.Failure("User not found.");
        }

        try
        {
            user.FirstName = firstName;
            user.LastName = lastName;

            await _authRepository.UpdateUserAsync(user);
            return ServiceResult.Success("Profile updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {Email}", email);
            return ServiceResult.Failure("Internal server error.");
        }
    }

    public Task<ServiceResult> LogoutAsync() =>
        Task.FromResult(ServiceResult.Success("Logout successful."));

    public async Task<ServiceResult> ChangePasswordAsync(string email, string oldPassword, string newPassword)
    {
        var user = await _authRepository.GetUserByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("Change password failed: User not found for email {Email}", email);
            return ServiceResult.Failure("User not found.");
        }

        if (user.Password != oldPassword)
        {
            _logger.LogWarning("Change password failed: Incorrect old password for user {Email}", email);
            return ServiceResult.Failure("Incorrect old password.");
        }

        var result = await _passwordValidator.ValidateAsync(newPassword);
        if (!result.IsValid)
        {
            var error = string.Join(" | ", result.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("New password validation failed for user {Email}: {Errors}", email, error);
            return ServiceResult.Failure(error);
        }

        try
        {
            user.Password = newPassword;
            await _authRepository.UpdateUserAsync(user);
            return ServiceResult.Success("Password changed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {Email}", email);
            return ServiceResult.Failure("Internal server error.");
        }
    }

    public async Task<ServiceResult> ResetPasswordAsync(string email, string newPassword)
    {
        var user = await _authRepository.GetUserByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("Reset password failed: User not found for email {Email}", email);
            return ServiceResult.Failure("User not found.");
        }

        var result = await _passwordValidator.ValidateAsync(newPassword);
        if (!result.IsValid)
        {
            var error = string.Join(" | ", result.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Reset password validation failed for user {Email}: {Errors}", email, error);
            return ServiceResult.Failure(error);
        }

        try
        {
            user.Password = newPassword;
            await _authRepository.UpdateUserAsync(user);
            return ServiceResult.Success("Password has been reset.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {Email}", email);
            return ServiceResult.Failure("Internal server error.");
        }
    }

    private string GenerateJWTToken(ApplicationUser user)
    {
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(key))
        {
            _logger.LogCritical("JWT key is missing in configuration.");
            throw new InvalidOperationException("JWT Key is not configured.");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.GivenName, user.FirstName ?? ""),
            new Claim(ClaimTypes.Surname, user.LastName ?? ""),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
