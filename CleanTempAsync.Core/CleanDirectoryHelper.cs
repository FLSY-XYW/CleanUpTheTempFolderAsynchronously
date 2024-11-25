using CleanTempAsync.Core.Helpers.InterfaceLists;

namespace CleanTempAsync.Core;

public class CleanDirectoryHelper
{
    private readonly IGetTempFolderPathHelper _getTempFolderPathHelper;

    public CleanDirectoryHelper(IGetTempFolderPathHelper getTempFolderPathHelper)
    {
        this._getTempFolderPathHelper = getTempFolderPathHelper;
    }

    #region 同步方法代码

    public void CleanDirectory()
    {
        string directoryPath = _getTempFolderPathHelper.GetTempFolderPath();

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Directory path is empty");
        }

        DirectoryInfo di = new DirectoryInfo(directoryPath);


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
        string directoryPath = _getTempFolderPathHelper.GetTempFolderPath();

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Directory path is empty");
        }

        DirectoryInfo di = new DirectoryInfo(directoryPath);

        // var tasks = new List<Task>();
        // FileInfo[] fileInfos = di.GetFiles("*.*");
        // foreach (var file in fileInfos)
        // {
        //     tasks.Add(TryDeleteFileAsync(file));
        // }
        //
        // DirectoryInfo[] directoryInfos = di.GetDirectories("*.*");
        //
        // foreach (DirectoryInfo? directoryInfo in directoryInfos)
        // {
        //     tasks.Add(TryDeleteDirectoryAsync(directoryInfo));
        // }
        //
        // await Task.WhenAll(tasks);

        var tasksFile = di.GetFiles("*.*").Select(file => TryDeleteFileAsync(file));
        var tasksDirectoryInfo =
            di.GetDirectories("*.*").Select(directoryInfo => TryDeleteDirectoryAsync(directoryInfo));

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
            // Task tryDeleteDirectoryAsync = Task.CompletedTask;
            // // 递归删除子目录
            // if (dir.GetDirectories().Length > 0)
            // {
            //     foreach (DirectoryInfo subDir in dir.GetDirectories())
            //     {
            //         tryDeleteDirectoryAsync = TryDeleteDirectoryAsync(subDir);
            //     }
            // }
            //
            // // 删除目录中的文件
            // await Task.WhenAll(dir.GetFiles().Select(file => TryDeleteFileAsync(file)));
            // await tryDeleteDirectoryAsync;
            // // 删除目录
            // dir.Delete(true);

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