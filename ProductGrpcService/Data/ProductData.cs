namespace ProductGrpcService.Data
{
    public class ProductData
    {
        public static List<ProductResponse> GetMockProductList()
        {
            return new List<ProductResponse>
            {
                new ProductResponse
                {
                    Id = 1,
                    Name = "Wireless Gaming Mouse",
                    Description = "High precision RGB mouse",
                    Price = 59.99,
                    CategoryId = 1,
                    InventoryId = 1,
                },
                new ProductResponse
                {
                    Id = 2,
                    Name = "Mechanical Keyboard",
                    Description = "Blue switch tactile keyboard",
                    Price = 89.99,
                    CategoryId = 2,
                    InventoryId= 1,
                },
                new ProductResponse
                {
                    Id = 3,
                    Name = "UltraWide 4K Monitor",
                    Description = "34-inch curved display",
                    Price = 399.99,
                    CategoryId = 1,
                    InventoryId=1
                },
                new ProductResponse
                {
                    Id = 4,
                    Name = "Noise-Cancelling Headset",
                    Description = "Over-ear wireless audio",
                    Price = 149.99,
                    CategoryId = 1,
                    InventoryId=1
                },
                new ProductResponse
                {
                    Id = 5,
                    Name = "USB-C Hub Multiport",
                    Description = "7-in-1 adapter dongle",
                    Price = 29.99,
                    CategoryId = 1,
                    InventoryId = 1,
                }
            };
        }
    }
}
