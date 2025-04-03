using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace OrderManagement.API.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _jwtKey;
        private IConfiguration _configuration;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
            _apiKey = configuration["ApiSettings:ApiKey"] ?? throw new ArgumentNullException("ApiSettings:ApiKey configuration is missing");
            _apiSecret = configuration["ApiSettings:ApiSecret"] ?? throw new ArgumentNullException("ApiSettings:ApiSecret configuration is missing");
            _jwtKey = configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key configuration is missing");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            if (ShouldSkipAuth(path))
            {
                await _next(context);
                return;
            }

            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                if (!await ValidateJwtToken(context)) return;
            }
            else
            {
                if (!ValidateApiKey(context)) return;
            }

            await _next(context);
        }

        private bool ShouldSkipAuth(string path)
        {
            return path != null && (
                path.StartsWith("/swagger") ||
                path.Contains("/auth/register") ||
                path.Contains("/auth/login"));
        }

        private async Task<bool> ValidateJwtToken(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();

            if (!authHeader.StartsWith("Bearer "))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Geçersiz token formatı. 'Bearer <token>' formatını kullanın.");
                return false;
            }

            var jwtToken = authHeader["Bearer ".Length..].Trim();

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtKey);

                tokenHandler.ValidateToken(jwtToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return true;
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync($"Geçersiz token: {ex.Message}");
                return false;
            }
        }

        private bool ValidateApiKey(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("X-API-Key", out var extractedApiKey) ||
                !context.Request.Headers.TryGetValue("X-API-Secret", out var extractedApiSecret))
            {
                context.Response.StatusCode = 401;
                context.Response.WriteAsync("API Key/Secret eksik. Ya JWT token ya da API Key/Secret sağlamalısınız.").Wait();
                return false;
            }

            if (!_apiKey.Equals(extractedApiKey) || !_apiSecret.Equals(extractedApiSecret))
            {
                context.Response.StatusCode = 401;
                context.Response.WriteAsync("Geçersiz API Key veya Secret.").Wait();
                return false;
            }

            return true;
        }
    }
} 