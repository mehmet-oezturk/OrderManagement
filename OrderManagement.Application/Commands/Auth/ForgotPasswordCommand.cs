using MediatR;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Commands.Auth;

public class ForgotPasswordCommand : IRequest<bool>
{
    public string Email { get; set; }
} 