using CategoryGrpcService;
using Grpc.Core;
using ProductGrpcService.Data;

namespace ProductGrpcService.Services
{
    public class CategoriesGrpcServiceImpl
    : global::CategoryGrpcService.CategoryService.CategoryServiceBase
    {
        private readonly ILogger<CategoriesGrpcServiceImpl> _logger;

        public CategoriesGrpcServiceImpl(
            ILogger<CategoriesGrpcServiceImpl> logger)
        {
            _logger = logger;
        }

        public override Task<
     global::CategoryGrpcService.CategoryListResponse>
     GetAllCategories(
         global::CategoryGrpcService.EmptyRequest request,
         global::Grpc.Core.ServerCallContext context)
        {
            _logger.LogInformation("Fetching all categories...");

            var response = new global::CategoryGrpcService.CategoryListResponse();

            response.Categories.AddRange(
                CategoryData.GetMockCategoryList()
            );

            return Task.FromResult(response);
        }
        public override Task<
    global::CategoryGrpcService.CategoryResponse>
    GetCategoryById(
        global::CategoryGrpcService.CategoryByIdRequest request,
        global::Grpc.Core.ServerCallContext context)
        {
            var category = CategoryData.GetMockCategoryList()
                .FirstOrDefault(c => c.Id == request.Id);

            if (category == null)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.NotFound,
                        $"Category {request.Id} not found."
                    )
                );
            }

            return Task.FromResult(category);
        }
    }
}