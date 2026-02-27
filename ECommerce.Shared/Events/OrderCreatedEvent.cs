using System;
using System.Collections.Generic;

namespace ECommerce.Shared.Events
{
    public class OrderCreatedEvent
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public List<OrderCreatedEventItem> Products { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
