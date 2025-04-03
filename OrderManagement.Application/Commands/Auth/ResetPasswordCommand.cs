using MediatR;

namespace OrderManagement.Application.Commands.Auth;

public class ResetPasswordCommand : IRequest<bool>
{
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
} 