using MediatR;
using OrderManagement.Core.Interfaces;
using OrderManagement.Application.Commands.Products;
using OrderManagement.Infrastructure.Services;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Handlers.Products;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductService _productService;

    public UpdateProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(request.Id);
        if (product == null)
            return null;

        var updatedProduct = new UpdateProductDto
        {
            Category = request.Category,
            Description = request.Description,
            IsActive = request.IsActive,
            Name = request.Name,
            Price = request.Price,
            StockQuantity = request.StockQuantity
        };

        return await _productService.UpdateProductAsync(request.Id,updatedProduct);
    }
} 