namespace server.Models
{
    public enum OrderStatus
    {
        Pending,
        Paid,
        Failed
    }

    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal TotalAmount { get; set; }
        public string? StripePaymentIntentId { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
    }

}