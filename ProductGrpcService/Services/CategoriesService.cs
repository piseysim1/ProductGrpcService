
using Grpc.Core;
using ProductGrpcService.Data;

namespace ProductGrpcService.Services
{
    public class CategoriesService : CategoryService.CategoryServiceBase
    {
        private readonly ILogger<CategoriesService> _logger;

        public CategoriesService(ILogger<CategoriesService> logger)
        {
            _logger = logger;
        }

        public override Task<CategoryListResponse> GetAllCategories(
            GetAllCategoriesRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Fetching all categories...");

            var response = new CategoryListResponse();

            response.Categories.AddRange(
                CategoryData.GetMockCategoryList());

            return Task.FromResult(response);
        }

        public override Task<CategoryResponse> GetCategoryById(
            CategoryByIdRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation(
                "Fetching category ID: {Id}", request.Id);

            var category = CategoryData.GetMockCategoryList()
                .FirstOrDefault(c => c.Id == request.Id);

            if (category == null)
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound, "Category not found."));
            }
                    
            return Task.FromResult(category);
        }
    }
}