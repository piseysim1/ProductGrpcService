using Grpc.Core;
using CategoryGrpcService;
using ProductGrpcService.Data;
namespace ProductGrpcService.Services
{
    public class ProductsGrpcServiceImpl : ProductService.ProductServiceBase
    {
        private readonly ILogger<ProductsGrpcServiceImpl> _logger;

        public ProductsGrpcServiceImpl(ILogger<ProductsGrpcServiceImpl> logger) => _logger = logger;
        public override Task<ProductListResponse> GetAllProducts(EmptyRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Fetching all products...");

            var response = new ProductListResponse();

            response.Product.AddRange(GetMockProductList());

            return Task.FromResult(response);
        }

        public override Task<FullProductDetailsResponse> GetProductById(ProductByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Fetching product details for ID: {Id}", request.Id);

            var product = GetMockProductList().FirstOrDefault(p => p.Id == request.Id)
                              ?? GetMockProductList().First();

            var category = CategoryData.GetMockCategoryList().FirstOrDefault(c => c.Id == product.CategoryId)
                             ?? CategoryData.GetMockCategoryList().First();
            
            

            var inventory = new InventoryResponse
            {
                Id = 100 + request.Id,
                ProductId = product.Id,
                StockQuantity = 45,
            };
            var response = new FullProductDetailsResponse
            {
                Products = product,
                Inventory = inventory,
                Category = category,
                
            };
            
            return Task.FromResult(response);
        }
        
        
       

        private List<ReviewResponse> GetMockReviewList()
        {
            return new List<ReviewResponse>
            {
                new ReviewResponse
                {
                    Id = 1,
                    Rating = 5,
                    Comment = "Super smooth!",
                    ReviewerName = "Alice"
                },
                new ReviewResponse
                {
                    Id = 2,
                    Rating = 4,
                    Comment = "Great value.",
                    ReviewerName = "Bob"
                }
            };
        }
       
        
        private List<ProductResponse> GetMockProductList()
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
                },
                new ProductResponse 
                { 
                    Id = 2, 
                    Name = "Mechanical Keyboard", 
                    Description = "Blue switch tactile keyboard", 
                    Price = 89.99,
                    CategoryId = 2,
                },
                new ProductResponse 
                { 
                    Id = 3, 
                    Name = "UltraWide 4K Monitor", 
                    Description = "34-inch curved display", 
                    Price = 399.99,
                    CategoryId = 3,
                },
                new ProductResponse 
                { 
                    Id = 4, 
                    Name = "Noise-Cancelling Headset", 
                    Description = "Over-ear wireless audio", 
                    Price = 149.99,
                    CategoryId = 1,
                },
                new ProductResponse 
                { 
                    Id = 5, 
                    Name = "USB-C Hub Multiport", 
                    Description = "7-in-1 adapter dongle", 
                    Price = 29.99,
                    CategoryId = 4,
                }
            };
        }
    }
}