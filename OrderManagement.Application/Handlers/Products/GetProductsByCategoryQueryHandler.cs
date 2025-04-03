using MediatR;
using OrderManagement.Core.DTOs;
using OrderManagement.Core.Interfaces;
using OrderManagement.Application.Queries.Products;

namespace OrderManagement.Application.Handlers.Products;

public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, List<ProductDto>>
{
    private readonly IProductService _productService;

    public GetProductsByCategoryQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<List<ProductDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var products = await _productService.GetProductsByCategoryAsync(request.Category);
        return products.ToList();
    }
} 