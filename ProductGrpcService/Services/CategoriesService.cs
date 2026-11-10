
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProductGrpcService.Messaging;

namespace ProductGrpcService.Services
{
    public class CategoriesService(ILogger<CategoriesService> logger, SampleDbContext db, IMapper mapper, RabbitMQProducer producer) : CategoryService.CategoryServiceBase
    {
        private readonly ILogger<CategoriesService> _logger = logger;
        private readonly SampleDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly RabbitMQProducer _producer = producer;


        public override async Task<CategoryListResponse> GetAllCategories(
            GetAllCategoriesRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Fetching all categories...");

            var CategoryEntity = await _db.Categories.ToListAsync();
            var cateogryRes = new CategoryListResponse();
            var mapCategory = _mapper.Map<IEnumerable<CategoryResponse>>(CategoryEntity);
            cateogryRes.Categories.AddRange(mapCategory);
            await _db.SaveChangesAsync();
            return cateogryRes;
        }

        public override async Task<CategoryResponse> GetCategoryById(
            CategoryByIdRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation(
                "Fetching category ID: {Id}", request.Id);

            var category = await _db.Categories.FindAsync(request.Id);

            if (category == null)
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound, "Category not found."));
            }

            return _mapper.Map<CategoryResponse>(category);
        }
    }
}