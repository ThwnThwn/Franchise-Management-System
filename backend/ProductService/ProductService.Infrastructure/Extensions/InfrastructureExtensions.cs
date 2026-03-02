using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Interfaces;
using ProductService.Application.Repositories;
using ProductService.Application.SearchEngines;
using ProductService.Infrastructure.ApplicationDbContext;
using ProductService.Infrastructure.Interceptor;
using ProductService.Infrastructure.Locks;
using ProductService.Infrastructure.Messaging;
using ProductService.Infrastructure.Repositories;
using ProductService.Infrastructure.SearchEngines;
using ProductService.Infrastructure.Services;
using ProductService.Infrastructure.Settings;
using ProductService.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructures(this IServiceCollection services, IConfiguration configuration)
        {
            // Connect Db
            var connectionString = configuration["PRODUCT_DB_CONNECTION"];
            // EF Core interceptors used by ProductDbContext
            services.AddScoped<UpdateTimestampInterceptor>();

            // DbContext + wire interceptor via DI
            services.AddDbContext<ProductDbContext>(
                (sp, opt) =>
                {
                    opt.UseNpgsql(connectionString);
                    opt.AddInterceptors(sp.GetRequiredService<UpdateTimestampInterceptor>());
                });
            // Expose DbContext via abstraction used by repositories
            services.AddScoped<IProductDbContext>(sp => sp.GetRequiredService<ProductDbContext>());

            // Register repositories
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
            // Elastic search
            services.AddElasticAndCapServices(configuration);

            // Register Cloudinary service
            services.AddScoped<ICloudinaryService, CloudinaryService>();

            // Cloudinary configuration
            services.Configure<CloudinarySettings>(options =>
            {
                options.CloudName = configuration["CLOUDINARY_CLOUD_NAME"] ?? string.Empty;
                options.ApiKey = configuration["CLOUDINARY_API_KEY"] ?? string.Empty;
                options.ApiSecret = configuration["CLOUDINARY_API_SECRET"] ?? string.Empty;
            });
            return services;
        }

        public static IServiceCollection AddElasticAndCapServices(this IServiceCollection services, IConfiguration configuration)
        {
            // ==========================================
            // 1.2 ELASTICSEARCH CLIENT
            // ==========================================
            var elasticUri = configuration["ELASTICSEARCH_URI"] ?? "http://localhost:9200";
            var settings = new ElasticsearchClientSettings(new Uri(elasticUri))
                .DefaultIndex("products");

            services.AddSingleton(new ElasticsearchClient(settings));
            services.AddScoped<IProductSearchEngine, ElasticProductSearchEngine>();

            // ==========================================
            // 1.3 DOTNETCORE.CAP (Message Broker & Outbox)
            // ==========================================
            var kafkaServers = configuration["KAFKA_BOOTSTRAP_SERVERS"] ?? "localhost:9092";
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                kafkaServers = "localhost:9092";
            }

            services.AddCap(x =>
            {
                // Tích hợp với EF Core để ăn chung Transaction
                x.UseEntityFramework<ProductDbContext>();

                // Lưu Outbox vào Postgres
                x.UsePostgreSql(configuration["PRODUCT_DB_CONNECTION"]!);

                // Cấu hình Kafka
                x.UseKafka(kafkaServers);

                // Bật Dashboard quản lý UI
                x.UseDashboard(opt =>
                {
                    opt.PathMatch = "/cap-ui"; // Truy cập chính xác qua https://localhost:5002/cap-ui
                    opt.StatsPollingInterval = 2000; // 2 giây cập nhật data 1 lần
                });

                // Cấu hình Retry khi lỗi mạng
                x.FailedRetryCount = 50;
                x.FailedRetryInterval = 60;
            });

            // ==========================================
            // ĐĂNG KÝ DISTRIBUTED LOCK
            // ==========================================
            services.AddScoped<ProductSyncSubscriber>();

            services.AddScoped<IProductUow, ProductUow>();
            services.AddScoped<IIntegrationEventBus, CapEventBus>();

            services.AddTransient<IDistributedLock, PostgresDistributedLock>();
            services.AddHostedService<ElasticBootstrapperWorker>();

            return services;
        }
    }
}