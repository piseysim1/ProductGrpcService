namespace ProductGrpcService.Messaging
{
    public class UpdateProductCommand
    {
        public string RequestId { get; set; } = Guid.NewGuid().ToString(); // uuid 

        public int ProductId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double Price { get; set; }

        public int CategoryId { get; set; }

        public int InventoryId { get; set; }
    }

}
