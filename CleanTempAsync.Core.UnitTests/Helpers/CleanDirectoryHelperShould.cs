using System.Diagnostics;
using System.Text.RegularExpressions;
using AutoFixture.Xunit2;
using AutoFixtureTesting.Shared;
using CleanTempAsync.Core.Helpers;
using CleanTempAsync.Core.Helpers.ImplementationClassLists;
using CleanTempAsync.Core.Helpers.InterfaceLists;
using FakeItEasy;
using FluentAssertions;
using NLog;
using NLog.Config;
using NLog.Shared.InterfaceLists;
using NLog.Targets;
using Xunit.Abstractions;

namespace CleanTempAsync.Core.UnitTests.Helpers;

public class CleanDirectoryHelperShould
{
    private readonly ITestOutputHelper _testOutputHelper;
    // private readonly string _tempFolderPath;

    public CleanDirectoryHelperShould(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        // this._tempFolderPath = Path.Combine(Path.GetTempPath(), "TestTempFolder");
    }

    [Theory]
    [AutoFakeItEasy]
    public void Given_TempFolderPath_And_Clean_The_Directory(
        [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
        [Frozen] ILoggerService<CleanDirectoryHelper> logger,
        CleanDirectoryHelper sut
    )
    {
        // Arrange

        #region Create temporary directories and files for testing/创建测试临时目录和文件

        string _tempFolderPath = Path.Combine(Path.GetTempPath(), "TestTempFolder");
        Directory.CreateDirectory(_tempFolderPath);
        File.Create(Path.Combine(_tempFolderPath, "file1.txt")).Dispose();
        File.Create(Path.Combine(_tempFolderPath, "file2.txt")).Dispose();
        File.Create(Path.Combine(_tempFolderPath, "file3.txt")).Dispose();
        File.Create(Path.Combine(_tempFolderPath, "file4.txt")).Dispose();
        Directory.CreateDirectory(Path.Combine(_tempFolderPath, "SubDir1"));
        Directory.CreateDirectory(Path.Combine(_tempFolderPath, "SubDir2"));
        Directory.CreateDirectory(Path.Combine(_tempFolderPath, "SubDir3"));
        Directory.CreateDirectory(Path.Combine(_tempFolderPath, "SubDir4"));
        File.Create(Path.Combine(_tempFolderPath, "SubDir1", "file1.txt")).Dispose();
        File.Create(Path.Combine(_tempFolderPath, "SubDir1", "file2.txt")).Dispose();
        File.Create(Path.Combine(_tempFolderPath, "SubDir2", "file3.txt")).Dispose();
        File.Create(Path.Combine(_tempFolderPath, "SubDir3", "file4.txt")).Dispose();
        File.Create(Path.Combine(_tempFolderPath, "SubDir4", "file5.txt")).Dispose();
        File.Create(Path.Combine(_tempFolderPath, "SubDir4", "file6.txt")).Dispose();

        #endregion

        A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(_tempFolderPath);
        // Act
        sut.CleanDirectory();
        // Assert
        Directory.Exists(_tempFolderPath).Should().BeFalse();
        // A.CallTo(() => logger.LogInformation(A<string>.That.Matches(msg =>
        //     Regex.IsMatch(msg, $@"^Directory {Regex.Escape(_tempFolderPath)} deleted$")))).MustHaveHappened();
        A.CallTo(() => logger.LogInformation(A<string>.That.Contains($"Directory {_tempFolderPath} deleted")))
            .MustHaveHappened();
    }

    [Theory]
    [AutoFakeItEasy]
    public void CleanDirectory_ShouldHandleUnauthorizedAccessException_WhenDeletingFiles(
        [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
        [Frozen] ILoggerService<CleanDirectoryHelper> logger,
        CleanDirectoryHelper sut
    )
    {
        // Arrange

        #region Create temporary directories and files for testing/创建测试临时目录和文件

        string _tempFolderPath = Path.Combine(Path.GetTempPath(), "TestTempFolder");
        // 创建测试临时目录和文件
        Directory.CreateDirectory(_tempFolderPath);
        var filePath = Path.Combine(_tempFolderPath, "file1.txt");
        File.Create(filePath).Dispose();
        // 使文件只读，模拟没有权限删除的情况
        var fileInfo = new FileInfo(filePath);
        fileInfo.IsReadOnly = true;

        #endregion

        A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(_tempFolderPath);
        // Act
        Action act = () => sut.CleanDirectory();
        // Assert
        act.Should().NotThrow(); // 确保没有抛出异常
        // 目录仍然存在，因为文件未被删除
        Directory.Exists(_tempFolderPath).Should().BeTrue();
        A.CallTo(() => logger.LogError(A<UnauthorizedAccessException>._,
                A<string>.That.Contains("No permission to delete the file")))
            .MustHaveHappened();
        A.CallTo(() => logger.LogError(A<IOException>._,
                A<string>.That.Contains("Directory in use or other IO error")))
            .MustHaveHappened();
        Dispose();
    }
    
    [Theory]
    [AutoFakeItEasy]
    public void CleanDirectory_ShouldHandleIOException_WhenDeletingDirectories(
        [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
        [Frozen] ILoggerService<CleanDirectoryHelper> logger,
        CleanDirectoryHelper sut
    )
    {
        // Arrange

        #region Create temporary directories and files for testing/创建测试临时目录和文件

        string _tempFolderPath = Path.Combine(Path.GetTempPath(), "TestTempFolder");
        // 创建测试临时目录和文件
        Directory.CreateDirectory(_tempFolderPath);
        var subDirPath = Path.Combine(_tempFolderPath, "SubDir");
        Directory.CreateDirectory(subDirPath);
        DirectoryInfo directoryInfo = new DirectoryInfo(subDirPath);
        directoryInfo.Attributes |= FileAttributes.ReadOnly;

        #endregion

        A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(_tempFolderPath);

        // Act
        Action act = () => sut.CleanDirectory();

        // Assert
        act.Should().NotThrow(); // 确保没有抛出异常
        // 目录仍然存在，因为可能发生了 IO 异常
        Directory.Exists(_tempFolderPath).Should().BeTrue();
        A.CallTo(() => logger.LogError(A<IOException>._,
                A<string>.That.Contains("Directory in use or other IO error")))
            .MustHaveHappened();
        A.CallTo(() => logger.LogError(A<IOException>._,
                A<string>.That.Contains("Directory in use or other IO error")))
            .MustHaveHappened();
        Dispose();
    }

    // #region Test for GetDirectory method/针对GetDirectory方法的测试
    //
    // [Theory]
    // [AutoFakeItEasy]
    // public void CleanDirectory_ShouldHandleArgumentException_WhenGetTempFolderPathReturnNull(
    //     [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
    //     [Frozen] ILoggerService<CleanDirectoryHelper> logger,
    //     CleanDirectoryHelper sut
    // )
    // {
    //     // Arrange
    //     A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(string.Empty);
    //     // Act
    //     Action act = () => sut.CleanDirectory();
    //
    //     // Assert
    //     // act.Should().Throw<ArgumentException>().WithMessage("Directory path is empty");
    //     A.CallTo(() => logger.LogError(A<AggregateException>._,
    //             A<string>.That.Contains("Directory path is empty")))
    //         .MustNotHaveHappened();
    // }
    //
    //
    // [Theory]
    // [AutoFakeItEasy]
    // public void CleanDirectory_ShouldHandleArgumentException_WhenGetTempFolderPathReturnNotExistPath(
    //     [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
    //     [Frozen] ILoggerService<CleanDirectoryHelper> logger,
    //     CleanDirectoryHelper sut,
    //     string fakeTempFolderPath
    // )
    // {
    //     // Arrange
    //     A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(fakeTempFolderPath);
    //     // Act
    //     Action act = () => sut.CleanDirectory();
    //
    //     // Assert
    //     A.CallTo(() => logger.LogError(A<DirectoryNotFoundException>._,
    //             A<string>.That.Contains("Directory path does not exist")))
    //         .MustNotHaveHappened();
    //     Dispose();
    // }
    //
    // [Theory]
    // [AutoFakeItEasy]
    // public void CleanDirectory_ShouldHandleArgumentException_WhenGetTempFolderPathReturnPathTooLong(
    //     [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
    //     [Frozen] ILoggerService<CleanDirectoryHelper> logger,
    //     CleanDirectoryHelper sut,
    //     string fakeTempFolderPath
    // )
    // {
    //     // Arrange
    //     A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(fakeTempFolderPath);
    //     // Act
    //     Action act = () => sut.CleanDirectory();
    //
    //     // Assert
    //     A.CallTo(() => logger.LogError(A<PathTooLongException>._,
    //             A<string>.That.Contains("Path too long")))
    //         .MustNotHaveHappened();
    //     Dispose();
    // }
    //
    // [Theory]
    // [AutoFakeItEasy]
    // public void CleanDirectory_ShouldHandleArgumentException_WhenGetTempFolderPathReturnNoPermission(
    //     [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
    //     [Frozen] ILoggerService<CleanDirectoryHelper> logger,
    //     CleanDirectoryHelper sut,
    //     string fakeTempFolderPath
    // )
    // {
    //     // Arrange
    //     A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(fakeTempFolderPath);
    //     // Act
    //     Action act = () => sut.CleanDirectory();
    //
    //     // Assert
    //     A.CallTo(() => logger.LogError(A<UnauthorizedAccessException>._,
    //             A<string>.That.Contains("No permission to access the path")))
    //         .MustNotHaveHappened();
    //     Dispose();
    // }
    //
    // [Theory]
    // [AutoFakeItEasy]
    // public void CleanDirectory_ShouldHandleArgumentException_WhenGetTempFolderPathReturnOtherException(
    //     [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
    //     [Frozen] ILoggerService<CleanDirectoryHelper> logger,
    //     CleanDirectoryHelper sut,
    //     string fakeTempFolderPath
    // )
    // {
    //     // Arrange
    //     A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(fakeTempFolderPath);
    //     // Act
    //     Action act = () => sut.CleanDirectory();
    //
    //     // Assert
    //     A.CallTo(() => logger.LogError(A<Exception>._,
    //             A<string>.That.Contains("An unexpected exception occurred")))
    //         .MustNotHaveHappened();
    //     Dispose();
    // }
    //
    // #endregion

    private void Dispose()
    {
        string tempFolderPath = Path.Combine(Path.GetTempPath(), "TestTempFolder");

        if (Directory.Exists(tempFolderPath))
        {
            // 获取目录中的所有文件和子目录
            var files = Directory.GetFiles(tempFolderPath, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                // 移除只读属性
                File.SetAttributes(file, FileAttributes.Normal);
            }

            string[] directories = Directory.GetDirectories(tempFolderPath, "*", SearchOption.AllDirectories);
            foreach (string directory in directories)
            {
                File.SetAttributes(directory, FileAttributes.Normal);
            }

            // 删除目录
            Directory.Delete(tempFolderPath, true);
        }
    }
}