namespace UserManagement.Application.Dtos;

public class ResetPasswordDto
{
    public string Email { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}