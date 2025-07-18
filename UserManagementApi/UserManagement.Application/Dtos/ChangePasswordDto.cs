namespace UserManagement.Application.Dtos;

public class ChangePasswordDto
{
    public string Email { get; set; } = default!;
    public string OldPassword { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}