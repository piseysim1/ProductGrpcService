using Grpc.Core;
using ProductGrpcService.Data;

namespace ProductGrpcService.Services
{
    public class InventorysService: InventoryService.InventoryServiceBase
    {
        private readonly ILogger<InventorysService> _logger;

        public InventorysService(ILogger<InventorysService> logger)
        {
            _logger = logger;
        }
        public override Task<InventoryListResponse> GetAllInventories(GetAllInventoryRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Fetching all Inventory...");


            var response = new InventoryListResponse();

            response.Inventory.AddRange(InventoryData.GetMockUpInventory());

            return Task.FromResult(response);
        }

        public override Task<InventoryResponse> GetInventoryById(InventoryByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Fetching inventory details for ID: {Id}", request.Id);

            var inventory = InventoryData.GetMockUpInventory().FirstOrDefault(p => p.Id == request.Id)
                              ?? InventoryData.GetMockUpInventory().First();
            if (inventory == null)
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound, "Product not found."));
            }

            return Task.FromResult(inventory);
        }
    }
}
