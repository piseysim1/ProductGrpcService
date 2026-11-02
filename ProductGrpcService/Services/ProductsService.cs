using Grpc.Core;
using ProductGrpcService.Data;
namespace ProductGrpcService.Services
{
    public class ProductsService : ProductService.ProductServiceBase
    {
        private readonly ILogger<ProductsService> _logger;

        public ProductsService(ILogger<ProductsService> logger) => _logger = logger;
        public override Task<ProductListResponse> GetAllProducts(GetAllProductsRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Fetching all products...");

            
            var response = new ProductListResponse();

            response.Product.AddRange(ProductData.GetMockProductList());

            return Task.FromResult(response);
        }

        public override Task<ProductResponse> GetProductById(ProductByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Fetching product details for ID: {Id}", request.Id);

            var product = ProductData.GetMockProductList().FirstOrDefault(p => p.Id == request.Id)
                              ?? ProductData.GetMockProductList().First();
            if (product == null)
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound, "Product not found."));
            }

            return Task.FromResult(product);
        }
    }
}