using System.Reflection;

namespace ProductGrpcService.Extension
{
    public static class ServiceExtensions
    {
        // 1. Extension method to automatically map all gRPC services
        public static WebApplication MapAllGrpcServices(this WebApplication app)
        {
            var grpcServiceTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.BaseType != null &&
                            t.BaseType.Name.EndsWith("ServiceBase"));

            var mapMethod = typeof(GrpcEndpointRouteBuilderExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "MapGrpcService" && m.GetParameters().Length == 1);

            foreach (var serviceType in grpcServiceTypes)
            {
                mapMethod.MakeGenericMethod(serviceType).Invoke(null, new object[] { app });
            }

            return app;
        }

        // 2. Extension method to keep GraphQL service configuration clean
        public static IServiceCollection AddCustomGraphQL(this IServiceCollection services)
        {
            services.AddGraphQLServer()
                .AddQueryType(d => d.Name("Query"))
                .AddTypesesFromAssembly(typeof(ServiceExtensions).Assembly);

            return services;
        }
    }
}
