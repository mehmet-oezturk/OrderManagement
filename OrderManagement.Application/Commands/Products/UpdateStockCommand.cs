using MediatR;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Commands.Products;

public class UpdateStockCommand : IRequest<ProductDto>
{
    public UpdateStockCommand(Guid id, int quantity)
    {
        Id = id;
        Quantity = quantity;
    }

    public Guid Id { get; set; }
    public int Quantity { get; set; }
} 