namespace ECommerce.Shared.Events
{
    public class OrderCreatedEventItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
