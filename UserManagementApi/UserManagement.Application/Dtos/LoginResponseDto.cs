namespace UserManagement.Application.Dtos;

public record LoginResponseDto(bool Flag, string Message = "", string Token = "");