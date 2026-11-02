using ProductGrpcService.Services;

namespace ProductGrpcService.Extension
{
    public static class ServiceExtensions
    {
        public static WebApplication MapAllGrpcServices(
            this WebApplication app)
        {
            app.MapGrpcService<ProductsService>();
            app.MapGrpcService<CategoriesService>();
            app.MapGrpcService<InventorysService>();
            return app;
        }
    }
}