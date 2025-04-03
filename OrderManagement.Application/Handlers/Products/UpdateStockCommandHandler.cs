using MediatR;
using OrderManagement.Core.DTOs;
using OrderManagement.Core.Interfaces;
using OrderManagement.Core.Entities;
using OrderManagement.Application.Commands.Products;

namespace OrderManagement.Application.Handlers.Products;

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, ProductDto>
{
    private readonly IProductService _productService;

    public UpdateStockCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<ProductDto> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(request.Id);
        if (product == null)
            return null;

        return await _productService.UpdateStockAsync(request.Id,request.Quantity);
    }
} 