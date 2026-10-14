namespace OrderService.API.Models;

public class Order
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}