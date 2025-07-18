using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Contracts;
using UserManagement.Application.Dtos;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserManagementController : ControllerBase
{
    private readonly IAuthService _authService;

    public UserManagementController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterUserDto registerUserDto)
    {
        if (registerUserDto == null)
        {
            return BadRequest("Invalid user data.");
        }

        var response = await _authService.RegisterUserAsync(registerUserDto);
        if (response.IsSuccess)
        {
            return Ok(response.Data);
        }

        return BadRequest(response.Message);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUserAsync([FromBody] LoginUserDto loginDto)
    {
        if (loginDto == null)
        {
            return BadRequest("Invalid login data.");
        }

        var response = await _authService.LoginUserAsync(loginDto);
        if (response.IsSuccess)
        {
            return Ok(response.Data);
        }

        return Unauthorized(response.Message);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var result = await _authService.LogoutAsync();
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var result = await _authService.ChangePasswordAsync(dto.Email, dto.OldPassword, dto.NewPassword);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var result = await _authService.ResetPasswordAsync(dto.Email, dto.NewPassword);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }
}