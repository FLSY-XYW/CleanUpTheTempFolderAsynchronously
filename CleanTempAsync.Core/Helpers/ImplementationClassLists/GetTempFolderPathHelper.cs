using CleanTempAsync.Core.Exceptions;
using CleanTempAsync.Core.Helpers.InterfaceLists;

namespace CleanTempAsync.Core.Helpers.ImplementationClassLists;

public class GetTempFolderPathHelper : IGetTempFolderPathHelper
{
    private readonly ITempFolderPathProvider _tempFolderPathProvider;

    public GetTempFolderPathHelper(ITempFolderPathProvider provider)
    {
        this._tempFolderPathProvider = provider;
    }

    public string GetTempFolderPath()
    {
        string? tempPath = _tempFolderPathProvider.TempFolderPathProvider();

        if (tempPath is null)
        {
            throw new GetTempFolderPathHelperException("Could not get temp folder path");
        }

        if (!Directory.Exists(tempPath))
        {
            throw new GetTempFolderPathHelperException("The temp folder path does not exist");
        }

        return tempPath;
    }
}