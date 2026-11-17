using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProductGrpcService.Observability;
using System.Diagnostics;
using static ProductGrpcService.Domain.SampleDb;

namespace ProductGrpcService.Services
{
    public class ProductsService(ILogger<ProductsService> logger,SampleDbContext db, IMapper mapper) : ProductService.ProductServiceBase
    {
        private readonly ILogger<ProductsService> _logger = logger;
        private readonly SampleDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        public override async Task<ProductListResponse> GetAllProducts(GetAllProductsRequest request, ServerCallContext context)
        {
            var products = await _db.Products.ToListAsync(context.CancellationToken);

            Activity.Current?.SetTag("product.count", products.Count);

            _logger.LogInformation("Retrieved {ProductCount} products", products.Count);

            var response = new ProductListResponse();
            response.Product.AddRange(_mapper.Map<IEnumerable<ProductResponse>>(products));

            return response;
        }
        public override async Task<ProductResponse> GetProductById(ProductByIdRequest request, ServerCallContext context)
        {

            var product = await _db.Products.FindAsync(new object[] { request.Id }, context.CancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} was not found", request.Id);
                throw new RpcException(new Status(StatusCode.NotFound, "Product not found."));
            }

            _logger.LogInformation("Product with ID {ProductId} retrieved successfully", product.Id);

            return _mapper.Map<ProductResponse>(product);
        }

        public override async Task<ProductResponse> AddProducts(AddProduct model, ServerCallContext context)
        {
            _logger.LogInformation( "Creating product with name {ProductName}", model.Name);

            var productEntity = _mapper.Map<Product>(model);
            _db.Products.Add(productEntity);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Product created successfully. ProductId: {ProductId}, ProductName: {ProductName}",productEntity.Id,productEntity.Name
            );
            return _mapper.Map<ProductResponse>(productEntity);
        }
    }
}