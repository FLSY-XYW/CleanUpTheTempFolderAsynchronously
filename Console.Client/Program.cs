// See https://aka.ms/new-console-template for more information

using CleanTempAsync.Core;
using CleanTempAsync.Core.Helpers.ImplementationClassLists;
using CleanTempAsync.Core.Helpers.InterfaceLists;
using Microsoft.Extensions.DependencyInjection;
using NLog.Shared.InterfaceLists;


// Console.WriteLine("Hello, World!");

ServiceCollection serviceCollection = new ServiceCollection();
serviceCollection.AddScoped<ITempFolderPathProvider, TempFolderPathProviderImpl>();
serviceCollection.AddScoped<IGetTempFolderPathHelper, GetTempFolderPathHelper>();
serviceCollection.AddMyLogger();
using (ServiceProvider buildServiceProvider = serviceCollection.BuildServiceProvider())
{
    IGetTempFolderPathHelper? getTempFolderPathHelper = buildServiceProvider.GetService<IGetTempFolderPathHelper>();
    ILoggerService<CleanDirectoryHelper>? logger =
        buildServiceProvider.GetService<ILoggerService<CleanDirectoryHelper>>();

    if ((getTempFolderPathHelper != null) && (logger != null))
    {
        Console.WriteLine(getTempFolderPathHelper!.GetTempFolderPath());
        CleanDirectoryHelper cleanDirectoryHelper = new CleanDirectoryHelper(getTempFolderPathHelper, logger);
        await cleanDirectoryHelper.CleanDirectoryAsync();
    }

    // Console.ReadKey();
}