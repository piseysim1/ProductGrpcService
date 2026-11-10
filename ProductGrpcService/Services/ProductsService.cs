using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProductGrpcService.Domain;
using ProductGrpcService.Messaging;
using ProductGrpcService.Model;
using static ProductGrpcService.Domain.SampleDb;
namespace ProductGrpcService.Services
{
    public class ProductsService(
        ILogger<ProductsService> logger,
        SampleDbContext db,
        IMapper mapper,
        MultiExchangeProducer multiProducer) : ProductService.ProductServiceBase
    {
        private readonly ILogger<ProductsService> _logger = logger;
        private readonly SampleDbContext _db = db;
        private readonly MultiExchangeProducer _multiProducer = multiProducer;
        private readonly IMapper _mapper = mapper;

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

            var productEntity = _mapper.Map<Product>(model);        
            _db.Products.Add(productEntity);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Product created successfully with ID: {Id}", productEntity.Id);
            return _mapper.Map<ProductResponse>(productEntity);
        }
      
        public override async Task<QueueAcceptedResponse> UpdateProductResponse(
            UpdateProductRequest request,
            ServerCallContext context)
        {

            if (request.Id <= 0 || string.IsNullOrWhiteSpace(request.Name))
            {
                throw new RpcException(
                    new Status(
                        StatusCode.InvalidArgument,
                        "Invalid product information."));
            }


            var command = new UpdateProductCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                ProductId = request.Id,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                CategoryId = request.CategoryId,
                InventoryId = request.InventoryId
            };



            //await _multiProducer.PublishFanoutAsync(command);
            var headers = new Dictionary<string, object> { { "format", "json" }, { "region", "asia" } };
            await _multiProducer.PublishHeadersAsync(command, headers);
            //await _multiProducer.PublishTopicAsync(command, routingKey: "product.europe.price-change");

            //_logger.LogInformation(
            //    "Product update command accepted. RequestId: {RequestId}",
            //          command.RequestId);


            return new QueueAcceptedResponse
            {
                RequestId = command.RequestId,
                Accepted = true,
                Message = "Product update command accepted for processing."
            };
        }
    }
}