using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace SharedKernel.Infrastructure.Kafka;

/// <summary>
/// Extension methods hỗ trợ đăng ký Kafka Consumer một cách ngắn gọn và chuẩn mực.
/// </summary>
public static class KafkaConsumerExtensions
{
    /// <summary>
    /// Đăng ký Kafka Consumer Service (Generic Background Service) và các Handler liên quan.
    /// </summary>
    /// <param name="services">IServiceCollection.</param>
    /// <param name="configure">Delegate để cấu hình Registry (map Topic -> Handler).</param>
    /// <returns>IServiceCollection để chain tiếp.</returns>
    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, Action<KafkaConsumerRegistry> configure)
    {
        var registry = new KafkaConsumerRegistry();
        configure(registry);

        services.AddSingleton(registry);
        services.AddHostedService<KafkaConsumerBackgroundService>();

        // Register handlers that are in the registry
        foreach (var topic in registry.GetAllTopics())
        {
            if (registry.TryGet(topic, out _, out var handlerType))
            {
                services.AddScoped(handlerType);
            }
        }

        return services;
    }
}
