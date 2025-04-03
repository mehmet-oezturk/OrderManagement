using MediatR;

namespace OrderManagement.Application.Commands.Products;

public class DeleteProductCommand : IRequest<bool>
{
    public Guid id;

    public DeleteProductCommand(Guid id)
    {
        this.id = id;
    }
} 