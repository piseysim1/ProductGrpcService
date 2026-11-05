using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProductGrpcService.Domain;
using ProductGrpcService.Messaging; 
namespace ProductGrpcService.Services
{
    public class ProductsService(
        ILogger<ProductsService> logger,
        SampleDbContext db,
        IMapper mapper,
        RabbitMQProducer producer) : ProductService.ProductServiceBase 
    {
        private readonly ILogger<ProductsService> _logger = logger;
        private readonly SampleDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly RabbitMQProducer _producer = producer; 

        public override async Task<ProductListResponse> GetAllProducts(GetAllProductsRequest request, ServerCallContext context)
        {
            var entity = await _db.Products.ToListAsync();

            var response = new ProductListResponse();

           
            var mappedProducts = _mapper.Map<IEnumerable<ProductResponse>>(entity);

            response.Product.AddRange(mappedProducts);
          
            await _db.SaveChangesAsync();
            return response;
        }

        public override async Task<ProductResponse> GetProductById(ProductByIdRequest request, ServerCallContext context)
        {
            var product = await _db.Products.FindAsync(request.Id);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Product not found."));
            }

            return _mapper.Map<ProductResponse>(product);
        }

        public override async Task<ProductResponse> AddProducts(AddProduct model, ServerCallContext context)
        {
            _logger.LogInformation("Creating a new product: {Name}", model.Name);

            var productEntity = _mapper.Map<SampleDb.Product>(model);
            _db.Products.Add(productEntity);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Product created successfully with ID: {Id}", productEntity.Id);
            return _mapper.Map<ProductResponse>(productEntity);
        }

        public override async Task<ProductResponse> UpdateProductResponse(UpdateProductRequest request, ServerCallContext context)
        {
           
            var product = await _db.Products.FindAsync(request.Id);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.Id} was not found."));
            }
            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.CategoryId = request.CategoryId;
            product.InventoryId = request.InventoryId;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Product ID {Id} updated in PostgreSQL database.", product.Id);


            string messagePayload = $"ProductUpdated:{product.Id},{product.Name},{DateTime.Now}";
            _producer.PublishProductEvent(messagePayload);

            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                InventoryId = product.InventoryId
            };
        }
    }
}