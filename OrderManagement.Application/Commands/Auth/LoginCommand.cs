using MediatR;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Commands.Auth;

public class LoginCommand : IRequest<AuthResponseDto>
{
    public string Email { get; set; }
    public string Password { get; set; }
} 