namespace ProductGrpcService.Data
{
    public class InventoryData
    {
        public static List<InventoryResponse> GetMockUpInventory()
        {
            return new List<InventoryResponse>
            {
                new InventoryResponse
                {
                    Id = 1,
                    ProductId = 1,
                    StockQuantity=10,
                    WarehoseLocation="Phnom Penh"
                }
            };
        }
    }
}
