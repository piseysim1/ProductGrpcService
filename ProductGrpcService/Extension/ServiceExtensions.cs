using System.Reflection;

namespace ProductGrpcService.Extension
{
    public static class ServiceExtensions
    {
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
    }
}
