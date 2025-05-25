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

        // Votre collection d'items
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        public decimal TotalAmount { get; set; }

        // Ancienne propriété pour PaymentIntent (vous pouvez la garder si vous en avez encore l’usage)
        public string? StripePaymentIntentId { get; set; }

        // Nouvelle propriété pour stocker la Session ID de Checkout
        public string? StripeSessionId { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
    }
}
