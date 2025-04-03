using MediatR;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Commands.Auth;

public class RefreshTokenCommand : IRequest<AuthResponseDto>
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
} 