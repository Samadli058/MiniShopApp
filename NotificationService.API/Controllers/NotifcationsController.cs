using Microsoft.AspNetCore.Mvc;
using NotificationService.API.Models;
using NotificationService.API.Services;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IEmailService _emailService;

    public NotificationsController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> Send(NotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required.");

        Console.WriteLine("----------------------------------");
        Console.WriteLine("NEW NOTIFICATION");
        Console.WriteLine($"Order Id: {request.OrderId}");
        Console.WriteLine($"Product: {request.ProductName}");
        Console.WriteLine($"Quantity: {request.Quantity}");
        Console.WriteLine($"Total: {request.TotalPrice} AZN");
        Console.WriteLine($"Email: {request.Email}");
        Console.WriteLine("----------------------------------");

        var subject = $"MiniShop - Order #{request.OrderId}";

        var body = $@"
            <h2>Your order has been created!</h2>
            <p><b>Order Id:</b> {request.OrderId}</p>
            <p><b>Product:</b> {request.ProductName}</p>
            <p><b>Quantity:</b> {request.Quantity}</p>
            <p><b>Total:</b> {request.TotalPrice} AZN</p>
            <p>Thank you for shopping with MiniShop.</p>";

        await _emailService.SendAsync(
            request.Email,
            subject,
            body);

        return Ok(new
        {
            message = "Notification sent successfully."
        });
    }
}