using MediatR;
using OrderManagement.Core.Interfaces;
using OrderManagement.Application.Commands.Auth;

namespace OrderManagement.Application.Handlers.Auth;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // TODO: Implement password reset logic
        return true;
    }
} 