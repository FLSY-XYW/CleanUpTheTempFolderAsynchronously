namespace NLog.Shared.InterfaceLists;

public interface ILoggerService<TSomeClass>
{
    void LogInformation(string message);

    void LogError(Exception ex, string message);
    // void LogWarning(string message);
}