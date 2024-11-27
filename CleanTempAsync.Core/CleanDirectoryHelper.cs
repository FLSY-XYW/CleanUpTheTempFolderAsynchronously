using CleanTempAsync.Core.Helpers.InterfaceLists;

namespace CleanTempAsync.Core;

public class CleanDirectoryHelper
{
    private readonly IGetTempFolderPathHelper _getTempFolderPathHelper;

    public CleanDirectoryHelper(IGetTempFolderPathHelper getTempFolderPathHelper)
    {
        this._getTempFolderPathHelper = getTempFolderPathHelper;
    }

    private bool GetDirectory(out DirectoryInfo? di)
    {
        string directoryPath = _getTempFolderPathHelper.GetTempFolderPath();

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Directory path is empty");
        }

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException("Directory path does not exist");
        }

        di = new DirectoryInfo(directoryPath);
        try
        {
            if (!di.Exists)
            {
                Console.WriteLine("Directory does not exist.");
            }
        }
        catch (ArgumentException ex)
        {
            // 路径格式不正确
            Console.WriteLine("ArgumentException: " + ex.Message);
            di = null;
        }
        catch (PathTooLongException ex)
        {
            // 路径太长
            Console.WriteLine("PathTooLongException: " + ex.Message);
            di = null;
        }
        catch (UnauthorizedAccessException ex)
        {
            // 没有权限访问路径
            Console.WriteLine("UnauthorizedAccessException: " + ex.Message);
            di = null;
        }
        // catch (DirectoryNotFoundException ex)
        // {
        //     // 路径不存在
        //     Console.WriteLine("DirectoryNotFoundException: " + ex.Message);
        // }
        catch (Exception ex)
        {
            // 其他类型的异常
            Console.WriteLine("An unexpected exception occurred: " + ex.Message);
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
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"No permission to delete directory: {di.FullName} - {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Directory in use or other IO error: {di.FullName} - {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting directory: {di.FullName} - {ex.Message}");
        }
    }

    private void TryDeleteFile(FileInfo file)
    {
        try
        {
            file.Delete();
            Console.WriteLine($"Deleted file: {file.FullName}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"No permission to delete file: {file.FullName} - {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"File in use or other IO error: {file.FullName} - {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file: {file.FullName} - {ex.Message}");
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
            dir.Delete(true);
            Console.WriteLine($"Deleted directory: {dir.FullName}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"No permission to delete directory: {dir.FullName} - {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Directory in use or other IO error: {dir.FullName} - {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting directory: {dir.FullName} - {ex.Message}");
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
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"No permission to delete directory: {di.FullName} - {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Directory in use or other IO error: {di.FullName} - {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting directory: {di.FullName} - {ex.Message}");
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

            await Task.WhenAll(subDirTasks);

            // 创建删除文件的任务，仅当文件不为空时
            var fileTasks = files.Length > 0
                ? files.Select(file => TryDeleteFileAsync(file))
                : Enumerable.Empty<Task>();

            // 同时删除文件和子目录
            await Task.WhenAll(fileTasks);


            dir.Delete(true);

            Console.WriteLine($"Deleted directory: {dir.FullName}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"No permission to delete directory: {dir.FullName} - {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Directory in use or other IO error: {dir.FullName} - {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting directory: {dir.FullName} - {ex.Message}");
        }
    }

    #endregion
}