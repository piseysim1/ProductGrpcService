using CategoryGrpcService;
using Grpc.Core;
using ProductGrpcService.Data;
namespace ProductGrpcService.Services
{
    public class CategoriesGrpcServiceImpl: CategoryService.CategoryServiceBase
    {
        private readonly ILogger<CategoriesGrpcServiceImpl> _logger;

        public CategoriesGrpcServiceImpl(ILogger<CategoriesGrpcServiceImpl> logger) => _logger = logger;

        public Task<CategoryListResponse> GetAllCategoryRequest (EmptyRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Fetching all categroies...");

            var response = new CategoryListResponse();

            response.Categories.AddRange(CategoryData.GetMockCategoryList());

            return Task.FromResult(response);
        }

        public override Task<CategoryResponse> GetCategoryById(CategoryByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Fetching product details for ID: {Id}", request.Id);

            var category = CategoryData.GetMockCategoryList().FirstOrDefault(c => c.Id == request.Id)
                             ?? CategoryData.GetMockCategoryList().First();
            return Task.FromResult(category);
        }

    }

}
