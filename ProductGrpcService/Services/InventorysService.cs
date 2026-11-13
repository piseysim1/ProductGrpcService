
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProductGrpcService.Messaging;

namespace ProductGrpcService.Services
{
    public class InventorysService(ILogger<InventorysService> logger, IMapper mapper,SampleDbContext db ) : InventoryService.InventoryServiceBase
    {
        private readonly ILogger<InventorysService> _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly SampleDbContext _db = db;

        public override async Task<InventoryListResponse> GetAllInventories( GetAllInventoryRequest request,ServerCallContext context)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching all inventories. Operation: {Operation}",
                    "GetAllInventories"
                );

                var entities = await _db.Inventories.ToListAsync();

                _logger.LogInformation(
                    "Retrieved {Count} inventories successfully.",
                    entities.Count
                );

                var response = new InventoryListResponse();

                var mappedInventories =
                    _mapper.Map<IEnumerable<InventoryResponse>>(entities);

                response.Inventory.AddRange(mappedInventories);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve inventories. Operation: {Operation}",
                    "GetAllInventories"
                );

                throw;
            }
        }

       
        
        public override async Task<InventoryResponse> GetInventoryById(InventoryByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation(
                "Fetching inventory with ID: {InventoryId}.",
                request.Id
            );

            var entity = await _db.Inventories.FindAsync(request.Id);

            if (entity == null)
            {
                _logger.LogWarning(
                    "Inventory with ID {InventoryId} was not found.",
                    request.Id
                );

                throw new RpcException(
                    new Status(StatusCode.NotFound, "Inventory not found.")
                );
            }

            _logger.LogInformation(
                "Inventory with ID {InventoryId} retrieved successfully.",
                entity.Id
            );

            return _mapper.Map<InventoryResponse>(entity);
        }
    }
}
