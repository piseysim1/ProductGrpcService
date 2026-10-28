using Grpc.Core;
using ProductGrpcService;

namespace ProductGrpcService.Services
{
    public class ProductsGrpcServiceImpl: ProductService.ProductServiceBase
    {
        private static readonly List<ProductResponse> _products = new()
        {
            new ProductResponse { Id = 1, Name = "Wireless Mouse", Price = 29.99, Category = "Accessories" },
            new ProductResponse { Id = 2, Name = "Mechanical Keyboard", Price = 89.99, Category = "Accessories" },
            new ProductResponse { Id = 3, Name = "UltraWide Monitor", Price = 349.99, Category = "Displays" }
        };
        public override Task<ProductListResponse> GetAllProducts(EmptyRequest request, ServerCallContext context)
        {
            var response = new ProductListResponse();
            response.Products.AddRange(_products);
            return Task.FromResult(response);
        }

        public override Task<ProductResponse> GetProductById(ProductByIdRequest request, ServerCallContext context)
        {
            var product = _products.FirstOrDefault(p => p.Id == request.Id);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));
            }
            return Task.FromResult(product);
        }

    }
}
