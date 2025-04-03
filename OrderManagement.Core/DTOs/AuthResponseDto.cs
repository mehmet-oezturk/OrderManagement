namespace OrderManagement.Core.DTOs;

public class AuthResponseDto
{
    public bool Succeeded { get; set; }
    public string[] Errors { get; set; } = Array.Empty<string>();
    public string? Token { get; set; }
} 