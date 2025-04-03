using MediatR;

namespace OrderManagement.Application.Commands.Auth;

public class LogoutCommand : IRequest<bool>
{
    public string UserId { get; set; }
} 