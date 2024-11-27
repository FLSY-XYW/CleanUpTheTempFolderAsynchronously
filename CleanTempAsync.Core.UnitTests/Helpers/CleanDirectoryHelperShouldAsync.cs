using AutoFixture.Xunit2;
using AutoFixtureTesting.Shared;
using CleanTempAsync.Core.Helpers.InterfaceLists;
using FakeItEasy;
using FluentAssertions;
using Xunit.Abstractions;

namespace CleanTempAsync.Core.UnitTests.Helpers;

public class CleanDirectoryHelperShouldAsync
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _tempFolderPath;

    public CleanDirectoryHelperShouldAsync(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        this._tempFolderPath = Path.Combine(Path.GetTempPath(), "TestTempFolder");
    }

    [Theory]
    [AutoFakeItEasy]
    public async Task Given_TempFolderPath_And_Clean_The_Directory_Asynchronously(
        [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
        CleanDirectoryHelper sut
    )
    {
        // Arrange

        #region Create temporary directories and files for testing/创建测试临时目录和文件

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
        await sut.CleanDirectoryAsync();
        // Assert
        Directory.Exists(_tempFolderPath).Should().BeFalse();
    }

    [Theory]
    [AutoFakeItEasy]
    public async Task CleanDirectory_ShouldHandleUnauthorizedAccessException_WhenDeletingFiles_Asynchronously(
        [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
        CleanDirectoryHelper sut
    )
    {
        // Arrange

        #region Create temporary directories and files for testing/创建测试临时目录和文件

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
        Func<Task> act = async () => await sut.CleanDirectoryAsync();

        // Assert
        await act.Should().NotThrowAsync(); // 确保没有抛出异常
        // 目录仍然存在，因为文件未被删除
        Directory.Exists(_tempFolderPath).Should().BeTrue();
        Dispose();
    }

    [Theory]
    [AutoFakeItEasy]
    public async Task CleanDirectory_ShouldHandleIOException_WhenDeletingDirectories_Asynchronously(
        [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
        CleanDirectoryHelper sut
    )
    {
        // Arrange

        #region Create temporary directories and files for testing/创建测试临时目录和文件

        // 创建测试临时目录和文件
        Directory.CreateDirectory(_tempFolderPath);
        var subDirPath = Path.Combine(_tempFolderPath, "SubDir");
        Directory.CreateDirectory(subDirPath);
        DirectoryInfo directoryInfo = new DirectoryInfo(subDirPath);
        directoryInfo.Attributes |= FileAttributes.ReadOnly;

        #endregion

        A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(_tempFolderPath);

        // Act
        Func<Task> act = async () => await sut.CleanDirectoryAsync();

        // Assert
        await act.Should().NotThrowAsync(); // 确保没有抛出异常
        // 目录仍然存在，因为可能发生了 IO 异常
        Directory.Exists(_tempFolderPath).Should().BeTrue();
        Dispose();
    }

    [Theory]
    [AutoFakeItEasy]
    public async Task CleanDirectory_ShouldHandleArgumentException_WhenGetTempFolderPathReturnNull_Asynchronously(
        [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
        CleanDirectoryHelper sut
    )
    {
        // Arrange
        A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(string.Empty);
        // Act
        Func<Task> act = async () => await sut.CleanDirectoryAsync();

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Directory path is empty");
    }

    [Theory]
    [AutoFakeItEasy]
    public async Task
        CleanDirectory_ShouldHandleArgumentException_WhenGetTempFolderPathReturnNotExistPath_Asynchronously(
            [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
            CleanDirectoryHelper sut,
            string fakeTempFolderPath
        )
    {
        // Arrange
        A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(fakeTempFolderPath);
        // Act
        Func<Task> act = async () => await sut.CleanDirectoryAsync();

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>().WithMessage("Directory path does not exist");
        Dispose();
    }

    [Fact]
    public void Dispose()
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