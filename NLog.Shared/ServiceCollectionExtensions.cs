using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Shared.ImplementationClassLists;
using NLog.Shared.InterfaceLists;

// namespace NLog.Shared;
// 把扩展方法放到依赖注入的命名空间下，可以直接使用，不用引用项目
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyLogger(this IServiceCollection services)
    {
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddNLog();
        });

        services.AddSingleton(typeof(ILoggerService<>), typeof(LoggerService<>));
        return services;
    }
}