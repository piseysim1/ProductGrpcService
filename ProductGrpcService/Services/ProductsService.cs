using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProductGrpcService.Domain;
using ProductGrpcService.Messaging;
using ProductGrpcService.Model;
using ProductGrpcService.Observability;
using RabbitMQ.Stream.Client.Reliable;
using System.Diagnostics;
using static ProductGrpcService.Domain.SampleDb;
namespace ProductGrpcService.Services
{
    public class ProductsService(
        ILogger<ProductsService> logger,
        SampleDbContext db,
        IMapper mapper,
        MultiExchangeProducer multiProducer) : ProductService.ProductServiceBase
    {
        private readonly ILogger<ProductsService> _logger = logger;
        private readonly SampleDbContext _db = db;
        private readonly MultiExchangeProducer _multiProducer = multiProducer;
        private readonly IMapper _mapper = mapper;

        //public override async Task<ProductListResponse> GetAllProducts(GetAllProductsRequest request, ServerCallContext context)
        //{
        //    _logger.LogDebug("Retrieving all products");
        //    var entity = await _db.Products.ToListAsync();

        //    var response = new ProductListResponse();


        //    var mappedProducts = _mapper.Map<IEnumerable<ProductResponse>>(entity);

        //    response.Product.AddRange(mappedProducts);
        //    _logger.LogDebug(
        //           "Retrieved {ProductCount} products",
        //           entity.Count
        //       );
        //    return response;
        //}

        public override async Task<ProductListResponse> GetAllProducts(
            GetAllProductsRequest request,
            ServerCallContext context)
        {
            // 1. TRACE: Create a custom span
            using var activity = Telemetry.ActivitySource
                .StartActivity("Product.GetAllProducts");

            // 2. LOG: Record operation started
            _logger.LogInformation("Retrieving all products");

            // 3. METRICS: Count requests
            Telemetry.GrpcRequests.Add(1,
                new KeyValuePair<string, object>(
                    "grpc.method", "GetAllProducts"
                )
            );

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Database query
                var entity = await _db.Products.ToListAsync();

                // Map Entity to gRPC Response
                var response = new ProductListResponse();

                var mappedProducts =
                    _mapper.Map<IEnumerable<ProductResponse>>(entity);

                response.Product.AddRange(mappedProducts);

                // LOG: Record successful retrieval
                _logger.LogInformation(
                    "Retrieved {ProductCount} products",
                    entity.Count
                );

                // TRACE: Add information to span
                activity?.SetTag("product.count", entity.Count);
                activity?.SetStatus(ActivityStatusCode.Ok);

                return response;
            }
            catch (Exception ex)
            {
                // TRACE: Record error
                activity?.SetStatus(
                    ActivityStatusCode.Error,
                    ex.Message
                );

                // LOG: Record exception
                _logger.LogError(
                    ex,
                    "Failed to retrieve all products"
                );

                throw;
            }
            finally
            {
                // METRICS: Record execution duration
                stopwatch.Stop();

                Telemetry.GrpcDuration.Record(
                    stopwatch.Elapsed.TotalMilliseconds,
                    new KeyValuePair<string, object>(
                        "grpc.method", "GetAllProducts"
                    )
                );
            }
        }


        public override async Task<ProductResponse> GetProductById(ProductByIdRequest request, ServerCallContext context)
        {
            _logger.LogDebug(
                   "Retrieving product with ID {ProductId}",
                   request.Id
                );
            var product = await _db.Products.FindAsync(request.Id);
            if (product == null)
            {
                _logger.LogWarning(
                "Product with ID {ProductId} was not found",
                request.Id
            );

                throw new RpcException(new Status(StatusCode.NotFound, "Product not found."));
            }
            _logger.LogDebug(
                "Product with ID {ProductId} retrieved successfully",
                product.Id
            );

            return _mapper.Map<ProductResponse>(product);
        }

        public override async Task<ProductResponse> AddProducts(AddProduct model, ServerCallContext context)
        {
            _logger.LogInformation(
                   "Creating product with name {ProductName}",
                   model.Name
               );

            var productEntity = _mapper.Map<Product>(model);
            _db.Products.Add(productEntity);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Product created successfully. ProductId: {ProductId}, ProductName: {ProductName}",
                productEntity.Id,
                productEntity.Name
            );
            return _mapper.Map<ProductResponse>(productEntity);
        }

        //public override async Task<QueueAcceptedResponse> UpdateProductResponse(
        //    UpdateProductRequest request,
        //    ServerCallContext context)
        //{

        //    if (request.Id <= 0 || string.IsNullOrWhiteSpace(request.Name))
        //    {
        //        throw new RpcException(
        //            new Status(
        //                StatusCode.InvalidArgument,
        //                "Invalid product information."));
        //    }


        //    var command = new UpdateProductCommand
        //    {
        //        RequestId = Guid.NewGuid().ToString(),
        //        ProductId = request.Id,
        //        Name = request.Name,
        //        Description = request.Description,
        //        Price = request.Price,
        //        CategoryId = request.CategoryId,
        //        InventoryId = request.InventoryId
        //    };



        //    //await _multiProducer.PublishFanoutAsync(command);
        //    var headers = new Dictionary<string, object> { { "format", "json" }, { "region", "asia" } };
        //    await _multiProducer.PublishHeadersAsync(command, headers);
        //    //await _multiProducer.PublishTopicAsync(command, routingKey: "product.europe.price-change");

        //    //_logger.LogInformation(
        //    //    "Product update command accepted. RequestId: {RequestId}",
        //    //          command.RequestId);


        //    return new QueueAcceptedResponse
        //    {
        //        RequestId = command.RequestId,
        //        Accepted = true,
        //        Message = "Product update command accepted for processing."
        //    };
        //}
        public override async Task<QueueAcceptedResponse> UpdateProductResponse(UpdateProductRequest request,ServerCallContext context)
        {
            if (request.Id <= 0 || string.IsNullOrWhiteSpace(request.Name))
            {
                _logger.LogWarning(
                    "Invalid product update request. ProductId: {ProductId}",
                    request.Id
                );

                throw new RpcException(
                    new Status(
                        StatusCode.InvalidArgument,
                        "Invalid product information."
                    )
                );
            }

            var command = new UpdateProductCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                ProductId = request.Id,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                CategoryId = request.CategoryId,
                InventoryId = request.InventoryId
            };

            var headers = new Dictionary<string, object>
        {
            { "format", "json" },
            { "region", "asia" }
        };

            _logger.LogInformation(
                "Publishing product update command. RequestId: {RequestId}, ProductId: {ProductId}",
                command.RequestId,
                command.ProductId
            );

            try
            {
                await _multiProducer.PublishHeadersAsync(command, headers);

                _logger.LogInformation(
                    "Product update command published successfully. RequestId: {RequestId}, ProductId: {ProductId}",
                    command.RequestId,
                    command.ProductId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to publish product update command. RequestId: {RequestId}, ProductId: {ProductId}",
                    command.RequestId,
                    command.ProductId
                );

                throw;
            }

            return new QueueAcceptedResponse
            {
                RequestId = command.RequestId,
                Accepted = true,
                Message = "Product update command accepted for processing."
            };
        }
    

    }
}