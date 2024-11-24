using CleanTempAsync.Core.Helpers.InterfaceLists;

namespace CleanTempAsync.Core.Helpers;

public class CleanDirectoryHelper
{
    private readonly IGetTempFolderPathHelper _getTempFolderPathHelper;

    public CleanDirectoryHelper(IGetTempFolderPathHelper getTempFolderPathHelper)
    {
        this._getTempFolderPathHelper = getTempFolderPathHelper;
    }

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
}
