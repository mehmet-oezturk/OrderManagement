using MediatR;

namespace OrderManagement.Application.Commands.Orders;

public class DeleteOrderCommand : IRequest<bool>
{
    public Guid id;

    public DeleteOrderCommand(Guid id)
    {
        this.id = id;
    }

} 