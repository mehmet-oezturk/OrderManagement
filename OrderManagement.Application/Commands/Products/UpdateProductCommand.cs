using MediatR;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Commands.Products;

public class UpdateProductCommand : IRequest<ProductDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; }
    public bool IsActive { get; set; }
} 