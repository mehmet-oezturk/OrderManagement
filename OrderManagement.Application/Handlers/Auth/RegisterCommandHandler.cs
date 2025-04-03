using MediatR;
using Microsoft.AspNetCore.Identity;
using OrderManagement.Core.DTOs;
using OrderManagement.Core.Entities;
using OrderManagement.Application.Commands.Auth;

namespace OrderManagement.Application.Handlers.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return new AuthResponseDto { Succeeded = false, Errors = result.Errors.Select(e => e.Description).ToArray() };

        return new AuthResponseDto { Succeeded = true };
    }
} 