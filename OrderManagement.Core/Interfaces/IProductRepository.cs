using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderManagement.Core.Entities;

namespace OrderManagement.Core.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(Guid id);
        Task<IEnumerable<Product>> GetByCategoryAsync(string category);
        Task<Product> AddAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task DeleteAsync(Guid id);
    }
} 