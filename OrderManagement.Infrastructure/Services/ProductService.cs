using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderManagement.Core.DTOs;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Interfaces;

namespace OrderManagement.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly RedisCacheService _cacheService;

        public ProductService(IProductRepository productRepository, RedisCacheService cacheService)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var cacheKey = "all_products";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var products = await _productRepository.GetAllAsync();
                return MapToDtos(products);
            });
        }

        public async Task<ProductDto> GetProductByIdAsync(Guid id)
        {
            var cacheKey = $"product:{id}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var product = await _productRepository.GetByIdAsync(id);
                return product != null ? MapToDto(product) : null;
            });
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category)
        {
            var cacheKey = $"products_category:{category}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var products = await _productRepository.GetByCategoryAsync(category);
                return MapToDtos(products);
            });
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            var product = new Product
            {
                Name = createProductDto.Name,
                Description = createProductDto.Description,
                Price = createProductDto.Price,
                Category = createProductDto.Category,
                StockQuantity = createProductDto.StockQuantity,
                CreatedAt = DateTime.UtcNow
            };

            var createdProduct = await _productRepository.AddAsync(product);
            await _cacheService.RemoveAsync("all_products");
            await _cacheService.RemoveAsync($"products_category:{product.Category}");

            return MapToDto(createdProduct);
        }

        public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return null;

            product.Name = updateProductDto.Name;
            product.Description = updateProductDto.Description;
            product.Price = updateProductDto.Price;
            product.Category = updateProductDto.Category;
            product.UpdatedAt = DateTime.UtcNow;

            var updatedProduct = await _productRepository.UpdateAsync(product);
            await _cacheService.RemoveAsync($"product:{id}");
            await _cacheService.RemoveAsync("all_products");
            await _cacheService.RemoveAsync($"products_category:{product.Category}");

            return MapToDto(updatedProduct);
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return false;

            await _productRepository.DeleteAsync(id);
            await _cacheService.RemoveAsync($"product:{id}");
            await _cacheService.RemoveAsync("all_products");
            await _cacheService.RemoveAsync($"products_category:{product.Category}");

            return true;
        }

        public async Task<ProductDto> UpdateStockAsync(Guid id, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return null;

            product.StockQuantity = quantity;
            product.UpdatedAt = DateTime.UtcNow;

            var updatedProduct = await _productRepository.UpdateAsync(product);
            await _cacheService.RemoveAsync($"product:{id}");
            await _cacheService.RemoveAsync("all_products");
            await _cacheService.RemoveAsync($"products_category:{product.Category}");

            return MapToDto(updatedProduct);
        }

        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Category = product.Category,
                StockQuantity = product.StockQuantity,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                IsActive = product.IsActive
            };
        }

        private IEnumerable<ProductDto> MapToDtos(IEnumerable<Product> products)
        {
            return products.Select(MapToDto);
        }
    }
} 