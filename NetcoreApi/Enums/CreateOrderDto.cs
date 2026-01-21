namespace NetcoreApi.Enums
{
    public class CreateOrderDto
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
