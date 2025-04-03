using MediatR;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Queries.Products;

public class GetProductsByCategoryQuery : IRequest<List<ProductDto>>
{
    public GetProductsByCategoryQuery(string category)
    {
        Category = category;
    }

    public string Category { get; set; }
} 