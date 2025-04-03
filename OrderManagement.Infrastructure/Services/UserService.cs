using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrderManagement.Core.Interfaces;

namespace OrderManagement.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetUserId()
        {
            return Guid.Parse(_httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        }

        public string GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value!;
        }
        public string GetUserNameSurname()
        {
            string name = _httpContextAccessor.HttpContext?.User?.FindFirst("Name")?.Value!;
            string surname = _httpContextAccessor.HttpContext?.User?.FindFirst("Surname")?.Value!;
            string nameSurname = name + " " + surname;
            return nameSurname;
        }
    }
}
