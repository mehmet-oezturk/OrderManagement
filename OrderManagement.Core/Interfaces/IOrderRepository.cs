using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderManagement.Core.Entities;

namespace OrderManagement.Core.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order> GetByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetByCustomerIdAsync(string customerId);
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId, int page = 1, int pageSize = 10);
        Task<Order> AddAsync(Order order);
        Task<Order> CreateAsync(Order order);
        Task<Order> UpdateAsync(Order order);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<Order>> GetByUserIdAsync(string userId);
    }
} 