using Microsoft.Extensions.Logging;
using NLog.Shared.InterfaceLists;
using NLog;

namespace NLog.Shared.ImplementationClassLists;

public class LoggerService<TSomeClass> : ILoggerService<TSomeClass>
{
    private readonly ILogger<TSomeClass> _logger;

    public LoggerService(ILogger<TSomeClass> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message)
    {
        _logger.LogInformation(message);
    }

    public void LogError(Exception ex, string message)
    {
        _logger.LogError(ex, message);
    }
}