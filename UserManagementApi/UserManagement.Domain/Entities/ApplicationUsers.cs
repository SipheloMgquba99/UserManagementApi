﻿namespace UserManagement.Domain.Entities;

public class ApplicationUser
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Roles Role { get; set; } = Roles.User; 
}

public enum Roles
{
    Admin,
    User,
    Trainer
}