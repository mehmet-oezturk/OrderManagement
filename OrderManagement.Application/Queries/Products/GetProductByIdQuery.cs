using MediatR;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Queries.Products;

public class GetProductByIdQuery : IRequest<ProductDto>
{
    public Guid id;

    public GetProductByIdQuery(Guid id)
    {
        this.id = id;
    }
} 