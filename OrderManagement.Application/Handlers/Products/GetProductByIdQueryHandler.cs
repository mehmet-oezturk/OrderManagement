using MediatR;
using OrderManagement.Core.DTOs;
using OrderManagement.Core.Interfaces;
using OrderManagement.Application.Queries.Products;

namespace OrderManagement.Application.Handlers.Products;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductService _productService;

    public GetProductByIdQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(request.id);
        return product;
    }
} 