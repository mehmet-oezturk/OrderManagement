namespace OrderManagement.Core.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public Order Order { get; set; }
        public Product Product { get; set; }
        public decimal TotalPrice { get; set; }

        public OrderItem()
        {
            Id = Guid.NewGuid();
        }
    }
} 