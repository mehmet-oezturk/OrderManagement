using MediatR;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Queries.Products;

public class GetProductsQuery : IRequest<List<ProductDto>>
{
} 