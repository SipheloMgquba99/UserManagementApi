using UserManagement.Application.Common;
using UserManagement.Application.Dtos;

namespace UserManagement.Application.Contracts;

public interface IAuthService
{
    Task<ServiceResult<RegistrationResponseDto>> RegisterUserAsync(RegisterUserDto registerUserDto);
    Task<ServiceResult<LoginResponseDto>> LoginUserAsync(LoginUserDto loginDto);
    Task<ServiceResult> SetupProfileAsync(string email, string firstName, string lastName);
    Task<ServiceResult> LogoutAsync();
    Task<ServiceResult> ChangePasswordAsync(string email, string oldPassword, string newPassword);
    Task<ServiceResult> ResetPasswordAsync(string email, string newPassword);
}