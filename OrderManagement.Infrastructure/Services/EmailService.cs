using OrderManagement.Core.Interfaces;

namespace OrderManagement.Infrastructure.Services;

public class EmailService : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // TODO: Implement actual email sending logic
        // For now, we'll just log the email details
        Console.WriteLine($"Sending email to: {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Body: {body}");
        
        await Task.CompletedTask;
    }
} 