using CleanTempAsync.Core.Helpers.InterfaceLists;
using NLog.Shared.InterfaceLists;

namespace CleanTempAsync.Core;

public class CleanDirectoryHelper
{
    private readonly IGetTempFolderPathHelper _getTempFolderPathHelper;
    private readonly ILoggerService<CleanDirectoryHelper> _logger;

    public CleanDirectoryHelper(IGetTempFolderPathHelper getTempFolderPathHelper,
        ILoggerService<CleanDirectoryHelper> logger)
    {
        this._getTempFolderPathHelper = getTempFolderPathHelper;
        this._logger = logger;
    }

    private bool GetDirectory(out DirectoryInfo? di)
    {
        string directoryPath = _getTempFolderPathHelper.GetTempFolderPath();

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            _logger.LogError(new AggregateException(), "Directory path is empty");
            // throw new ArgumentException("Directory path is empty");
        }

        if (!Directory.Exists(directoryPath))
        {
            _logger.LogError(new DirectoryNotFoundException(), "Directory path does not exist");
            // throw new DirectoryNotFoundException("Directory path does not exist");
        }

        di = new DirectoryInfo(directoryPath);
        try
        {
            if (!di.Exists)
            {
                _logger.LogError(new AggregateException(), "Directory does not exist.");
                // Console.WriteLine("Directory does not exist.");
            }
        }
        catch (ArgumentException ex)
        {
            // 路径格式不正确
            _logger.LogError(ex, "The path format is incorrect");
            // Console.WriteLine("ArgumentException: " + ex.Message);
            di = null;
        }
        catch (PathTooLongException ex)
        {
            // 路径太长
            _logger.LogError(ex, "Path too long");
            // Console.WriteLine("PathTooLongException: " + ex.Message);
            di = null;
        }
        catch (UnauthorizedAccessException ex)
        {
            // 没有权限访问路径
            _logger.LogError(ex, "No permission to access the path");
            // Console.WriteLine("UnauthorizedAccessException: " + ex.Message);
            di = null;
        }
        // catch (DirectoryNotFoundException ex)
        // {
        //     // 路径不存在
        //     Console.WriteLine("DirectoryNotFoundException: " + ex.Message);
        // }
        catch (Exception ex)
        {
            // 其他类型的异常 | 发生意外异常
            _logger.LogError(ex, "An unexpected exception occurred");
            // Console.WriteLine("An unexpected exception occurred: " + ex.Message);
            di = null;
        }

        return di != null;
    }

    #region 同步方法代码

    public void CleanDirectory()
    {
        if (!GetDirectory(out var di)) return;

        if (di == null)
        {
            return;
        }

        FileInfo[] fileInfos = di.GetFiles("*.*");
        foreach (var file in fileInfos)
        {
            TryDeleteFile(file);
        }

        DirectoryInfo[] directoryInfos = di.GetDirectories("*.*");

        foreach (DirectoryInfo? directoryInfo in directoryInfos)
        {
            TryDeleteDirectory(directoryInfo);
        }

        try
        {
            di.Delete();
            _logger.LogInformation($"Directory {di.FullName} deleted");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "No permission to delete the directory: {di.FullName} - {ex.Message}");
            // Console.WriteLine($"No permission to delete directory: {di.FullName} - {ex.Message}");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Directory in use or other IO error: {di.FullName} - {ex.Message}");
            // Console.WriteLine($"Directory in use or other IO error: {di.FullName} - {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting directory: {di.FullName} - {ex.Message}");
            // Console.WriteLine($"Error deleting directory: {di.FullName} - {ex.Message}");
        }
    }

    private void TryDeleteFile(FileInfo file)
    {
        try
        {
            file.Delete();
            _logger.LogInformation($"Deleted file: {file.FullName}");
            // Console.WriteLine($"Deleted file: {file.FullName}");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "No permission to delete the file: {file.FullName} - {ex.Message}");
            // Console.WriteLine($"No permission to delete file: {file.FullName} - {ex.Message}");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, $"File in use or other IO error: {file.FullName} - {ex.Message}");
            // Console.WriteLine($"File in use or other IO error: {file.FullName} - {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting file: {file.FullName} - {ex.Message}");
            // Console.WriteLine($"Error deleting file: {file.FullName} - {ex.Message}");
        }
    }

    private void TryDeleteDirectory(DirectoryInfo dir)
    {
        try
        {
            // 递归删除子目录
            if (dir.GetDirectories().Length > 0)
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    TryDeleteDirectory(subDir);
                }
            }

            // 删除目录中的文件
            foreach (FileInfo file in dir.GetFiles())
            {
                TryDeleteFile(file);
            }

            // 删除目录
            dir.Delete();
            _logger.LogInformation($"Deleted directory: {dir.FullName}");
            // Console.WriteLine($"Deleted directory: {dir.FullName}");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "No permission to delete the directory: {dir.FullName} - {ex.Message}");
            // Console.WriteLine($"No permission to delete directory: {dir.FullName} - {ex.Message}");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, $"Directory in use or other IO error: {dir.FullName} - {ex.Message}");
            // Console.WriteLine($"Directory in use or other IO error: {dir.FullName} - {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting directory: {dir.FullName} - {ex.Message}");
            // Console.WriteLine($"Error deleting directory: {dir.FullName} - {ex.Message}");
        }
    }

    #endregion

    #region 异步方法

    public async Task CleanDirectoryAsync()
    {
        if (!GetDirectory(out var di)) return;

        if (di == null)
        {
            return;
        }

        FileInfo[] fileInfos = di.GetFiles("*.*");
        DirectoryInfo[] directoryInfos = di.GetDirectories("*.*");

        var tasksFile = fileInfos.Length > 0
            ? fileInfos.Select(file => TryDeleteFileAsync(file))
            : Enumerable.Empty<Task>();

        var tasksDirectoryInfo = directoryInfos.Length > 0
            ? directoryInfos.Select(directoryInfo => TryDeleteDirectoryAsync(directoryInfo))
            : Enumerable.Empty<Task>();

        await Task.WhenAll(tasksFile.Concat(tasksDirectoryInfo));

        try
        {
            di.Delete();
            _logger.LogInformation($"Deleted directory: {di.FullName}");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "No permission to delete the directory: {di.FullName} - {ex.Message}");
            // Console.WriteLine($"No permission to delete directory: {di.FullName} - {ex.Message}");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, $"Directory in use or other IO error: {di.FullName} - {ex.Message}");
            // Console.WriteLine($"Directory in use or other IO error: {di.FullName} - {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting directory: {di.FullName} - {ex.Message}");
            // Console.WriteLine($"Error deleting directory: {di.FullName} - {ex.Message}");
        }
    }

    private Task TryDeleteFileAsync(FileInfo file)
    {
        return Task.Run((() => TryDeleteFile(file)));
    }

    private async Task TryDeleteDirectoryAsync(DirectoryInfo dir)
    {
        try
        {
            DirectoryInfo[] directories = dir.GetDirectories();
            FileInfo[] files = dir.GetFiles();

            // 创建删除子目录的任务，仅当子目录不为空时
            var subDirTasks = directories.Length > 0
                ? directories.Select(subDir => TryDeleteDirectoryAsync(subDir))
                : Enumerable.Empty<Task>();

            // 创建删除文件的任务，仅当文件不为空时
            var fileTasks = files.Length > 0
                ? files.Select(file => TryDeleteFileAsync(file))
                : Enumerable.Empty<Task>();

            // 同时删除文件和子目录
            await Task.WhenAll(fileTasks);
            await Task.WhenAll(subDirTasks);

            dir.Delete();

            Console.WriteLine($"Deleted directory: {dir.FullName}");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "No permission to delete the directory: {dir.FullName} - {ex.Message}");
            // Console.WriteLine($"No permission to delete directory: {dir.FullName} - {ex.Message}");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, $"Directory in use or other IO error: {dir.FullName} - {ex.Message}");
            // Console.WriteLine($"Directory in use or other IO error: {dir.FullName} - {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting directory: {dir.FullName} - {ex.Message}");
            // Console.WriteLine($"Error deleting directory: {dir.FullName} - {ex.Message}");
        }
    }

    #endregion
}