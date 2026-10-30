using ProductGrpcService.Services;

namespace ProductGrpcService.Extension
{
    public static class ServiceExtensions
    {
        public static WebApplication MapAllGrpcServices(
            this WebApplication app)
        {
            app.MapGrpcService<ProductsGrpcServiceImpl>();

            app.MapGrpcService<CategoriesGrpcServiceImpl>();

            return app;
        }
    }
}