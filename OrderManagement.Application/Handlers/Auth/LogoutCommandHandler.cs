using MediatR;
using Microsoft.AspNetCore.Identity;
using OrderManagement.Application.Commands.Auth;
using OrderManagement.Core.Entities;

namespace OrderManagement.Application.Handlers.Auth;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LogoutCommandHandler(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _signInManager.SignOutAsync();
        return true;
    }
} 