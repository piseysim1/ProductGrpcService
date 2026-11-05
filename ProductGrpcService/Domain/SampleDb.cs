namespace ProductGrpcService.Domain
{
    public class SampleDb
    {
        public class Category
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
        public class Inventory
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int StockQuantity { get; set; }
            public string WarehouseLocation { get; set; }
        }
        public class Product
        {
            public int Id { get; set; }

            public string Name { get; set; } 
            public string Description { get; set; }
            public double Price { get; set; }
            public int CategoryId { get; set; }
            public int InventoryId { get; set; }

        }
    }
}
