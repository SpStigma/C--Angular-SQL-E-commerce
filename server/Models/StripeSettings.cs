namespace server.Models
{
    public class StripeSettings
    {
        public string? SecretKey { get; set; }
        public string? PublishableKey { get; set; }
        public string? Url { get; set; } 
    }
}
