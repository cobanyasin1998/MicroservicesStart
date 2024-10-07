using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Order.API.Models;
using Order.API.Models.Entities;
using Order.API.ViewModels;
using Shared.Events;

namespace Order.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly OrderAPIDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    public OrdersController(OrderAPIDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderVM createOrderVM)
    {
        Order.API.Models.Entities.Order order = new()
        {
            OrderId = Guid.NewGuid(),
            BuyerId = createOrderVM.BuyerId,
            CreatedDate = DateTime.Now,
            OrderStatu = Models.Enums.OrderStatus.Suspend
        };

        order.OrderItems = createOrderVM.OrderItems.Select(oi => new OrderItem
        {
            Count = oi.Count,
            Price = oi.Price,
            ProductId = oi.ProductId,
        }).ToList();

        order.TotalPrice = createOrderVM.OrderItems.Sum(oi => (oi.Price * oi.Count));

        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();

        OrderCreatedEvent orderCreatedEvent = new()
        {
            BuyerId = order.BuyerId,
            OrderId = order.OrderId,
            OrderItems = order.OrderItems.Select(oi => new Shared.Messages.OrderItemMessage
            {
                Count = oi.Count,
                ProductId = oi.ProductId,
            }).ToList(),
            TotalPrice = order.TotalPrice
        };

        await _publishEndpoint.Publish(orderCreatedEvent);

        return Ok();

    }
}
