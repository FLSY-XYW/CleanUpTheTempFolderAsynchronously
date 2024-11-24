using CleanTempAsync.Core.Helpers.InterfaceLists;

namespace CleanTempAsync.Core.Helpers.ImplementationClassLists;

public class TempFolderPathProviderImpl : ITempFolderPathProvider
{
    public string? TempFolderPathProvider()
    {
        string tempFileName = Path.GetTempFileName();
        string? tempPath = Path.GetDirectoryName(tempFileName);
        File.Delete(tempFileName); // 删除临时文件，只保留路径
        return tempPath;
    }
}