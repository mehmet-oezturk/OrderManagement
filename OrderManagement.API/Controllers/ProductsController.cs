using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using OrderManagement.Application.Queries;
using OrderManagement.Application.Commands;
using OrderManagement.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.RateLimiting;
using OrderManagement.Application.Queries.Products;
using OrderManagement.Application.Commands.Products;
using OrderManagement.Core.Entities;

namespace OrderManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var query = new GetProductsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(string id)
        {
            Guid productId = Guid.Empty;
            if (!string.IsNullOrEmpty(id))
            {
                productId = new Guid(id);
            }
            var query = new GetProductByIdQuery(productId);
            var result = await _mediator.Send(query);
            if (result == null)
                return NotFound();
            return Ok(result);

        }

        [HttpGet("category/{category}")]
        [EnableRateLimiting("JwtPolicy")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(string category)
        {
            var products = await _mediator.Send(new GetProductsByCategoryQuery(category));
            return Ok(products);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
        {
            if (id != command.Id)
                return BadRequest();

            var result = await _mediator.Send(command);
            if (result == null)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var command = new DeleteProductCommand(id);
            var result = await _mediator.Send(command);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id}/stock")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("JwtPolicy")]
        public async Task<ActionResult<ProductDto>> UpdateStock(Guid id, [FromBody] int quantity)
        {
            var command = new UpdateStockCommand(id, quantity);
            var result = await _mediator.Send(command);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
} 