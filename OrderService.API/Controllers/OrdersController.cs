using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.API.Data;
using OrderService.API.Models;
using OrderService.API.Services;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _context;
    private readonly IProductServiceClient _productService;
    private readonly INotificationServiceClient _notificationService;

    public OrdersController(
        OrderDbContext context,
        IProductServiceClient productService,
        INotificationServiceClient notificationService)
    {
        _context = context;
        _productService = productService;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _context.Orders
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _context.Orders
            .FindAsync(id);

        if (order is null)
            return NotFound("Order not found.");

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateOrderRequest request)
    {
        if (request.Quantity <= 0)
            return BadRequest(
                "Quantity must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required.");

        if (!MailAddress.TryCreate(request.Email, out _))
            return BadRequest("Email is not valid.");

        // 1. ProductService-dən məhsulu alırıq
        var product = await _productService
            .GetProductAsync(request.ProductId);

        if (product is null)
            return BadRequest("Product not found.");

        // 2. Stock yoxlayırıq
        if (product.Stock < request.Quantity)
            return BadRequest("Not enough stock.");

        // 3. Order yaradırıq
        var order = new Order
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = request.Quantity,
            UnitPrice = product.Price,
            TotalPrice = product.Price * request.Quantity,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);

        await _context.SaveChangesAsync();

        // 4. ProductService-ə stock azalt deyirik
        var stockReduced =
            await _productService.ReduceStockAsync(
                product.Id,
                request.Quantity);

        if (!stockReduced)
        {
            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            return BadRequest(
                "Order could not be completed because stock update failed.");
        }

        // 5. NotificationService çağırılır
        try
        {
            await _notificationService.SendAsync(
                new NotificationRequest
                {
                    OrderId = order.Id,
                    ProductName = order.ProductName,
                    Quantity = order.Quantity,
                    TotalPrice = order.TotalPrice,
                    Email = order.Email
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Notification could not be sent: {ex.Message}");
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = order.Id },
            order);
    }
}