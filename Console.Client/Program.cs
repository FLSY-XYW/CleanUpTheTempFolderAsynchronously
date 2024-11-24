// See https://aka.ms/new-console-template for more information

using CleanTempAsync.Core.Helpers;
using CleanTempAsync.Core.Helpers.ImplementationClassLists;
using CleanTempAsync.Core.Helpers.InterfaceLists;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

Console.WriteLine("Hello, World!");

ServiceCollection serviceCollection = new ServiceCollection();
serviceCollection.AddScoped<ITempFolderPathProvider, TempFolderPathProviderImpl>();
serviceCollection.AddScoped<IGetTempFolderPathHelper, GetTempFolderPathHelper>();
using (ServiceProvider buildServiceProvider = serviceCollection.BuildServiceProvider())
{
    IGetTempFolderPathHelper? getTempFolderPathHelper = buildServiceProvider.GetService<IGetTempFolderPathHelper>();

    if (getTempFolderPathHelper != null)
    {
        Console.WriteLine(getTempFolderPathHelper!.GetTempFolderPath());
        CleanDirectoryHelper cleanDirectoryHelper = new CleanDirectoryHelper(getTempFolderPathHelper);
        cleanDirectoryHelper.CleanDirectory();
    }
}