using MediatR;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Commands.Auth;

public class RegisterCommand : IRequest<AuthResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
} 