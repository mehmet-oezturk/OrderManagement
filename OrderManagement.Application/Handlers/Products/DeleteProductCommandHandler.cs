using MediatR;
using OrderManagement.Core.Interfaces;
using OrderManagement.Application.Commands.Products;

namespace OrderManagement.Application.Handlers.Products;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductService _productService;

    public DeleteProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(request.id);
        if (product == null)
            return false;

        return await _productService.DeleteProductAsync(product.Id);
    }
} 