using AutoMapper;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProductGrpcService.Messaging;

namespace ProductGrpcService.Services
{
    public class InventorysService(ILogger<InventorysService> logger, IMapper mapper, SampleDbContext db, RabbitMQProducer producer) : InventoryService.InventoryServiceBase
    {
        private readonly ILogger<InventorysService> _logger= logger;
        private readonly IMapper _mapper = mapper;
        private readonly SampleDbContext _db = db;
        private readonly RabbitMQProducer _rabbitmqProducer = producer;
        public override async Task<InventoryListResponse> GetAllInventories(GetAllInventoryRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Fetching all Inventory...");

            var entity = await _db.Inventories.ToListAsync();
            var InventoryRes = new InventoryListResponse();
            var mapInventory = _mapper.Map<IEnumerable<InventoryResponse>>(entity);
            InventoryRes.Inventory.AddRange(mapInventory);
            return InventoryRes;
        }


        public override async Task<InventoryResponse> GetInventoryById(InventoryByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Fetching inventory details for ID: {Id}", request.Id);

           var entity = await _db.Inventories.FindAsync(request.Id);

            if (entity == null)
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound, "Product not found."));
            }
            return _mapper.Map<InventoryResponse>(entity) ;
        }

        
    }
}
