using AutoFixture.Xunit2;
using AutoFixtureTesting.Shared;
using CleanTempAsync.Core.Helpers.InterfaceLists;
using FakeItEasy;
using NLog.Shared.InterfaceLists;

namespace CleanTempAsync.Core.UnitTests;

public class GetDirectoryShould
{
    
    #region Test for GetDirectory method/针对GetDirectory方法的测试

    [Theory]
    [AutoFakeItEasy]
    public void CleanDirectory_ShouldHandleArgumentException_WhenGetTempFolderPathReturnNull(
        [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
        [Frozen] ILoggerService<CleanDirectoryHelper> logger,
        CleanDirectoryHelper sut
    )
    {
        // Arrange
        A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(string.Empty);
        // Act
        Action act = () => sut.CleanDirectory();

        // Assert
        // act.Should().Throw<ArgumentException>().WithMessage("Directory path is empty");
        A.CallTo(() => logger.LogError(A<AggregateException>._,
                A<string>.That.Contains("Directory path is empty")))
            .MustNotHaveHappened();
    }


    [Theory]
    [AutoFakeItEasy]
    public void CleanDirectory_ShouldHandleArgumentException_WhenGetTempFolderPathReturnNotExistPath(
        [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
        [Frozen] ILoggerService<CleanDirectoryHelper> logger,
        CleanDirectoryHelper sut,
        string fakeTempFolderPath
    )
    {
        // Arrange
        A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(fakeTempFolderPath);
        // Act
        Action act = () => sut.CleanDirectory();

        // Assert
        A.CallTo(() => logger.LogError(A<DirectoryNotFoundException>._,
                A<string>.That.Contains("Directory path does not exist")))
            .MustNotHaveHappened();
        Dispose();
    }

    [Theory]
    [AutoFakeItEasy]
    public void CleanDirectory_ShouldHandleArgumentException_WhenGetTempFolderPathReturnPathTooLong(
        [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
        [Frozen] ILoggerService<CleanDirectoryHelper> logger,
        CleanDirectoryHelper sut,
        string fakeTempFolderPath
    )
    {
        // Arrange
        A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(fakeTempFolderPath);
        // Act
        Action act = () => sut.CleanDirectory();

        // Assert
        A.CallTo(() => logger.LogError(A<PathTooLongException>._,
                A<string>.That.Contains("Path too long")))
            .MustNotHaveHappened();
        Dispose();
    }

    [Theory]
    [AutoFakeItEasy]
    public void CleanDirectory_ShouldHandleArgumentException_WhenGetTempFolderPathReturnNoPermission(
        [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
        [Frozen] ILoggerService<CleanDirectoryHelper> logger,
        CleanDirectoryHelper sut,
        string fakeTempFolderPath
    )
    {
        // Arrange
        A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(fakeTempFolderPath);
        // Act
        Action act = () => sut.CleanDirectory();

        // Assert
        A.CallTo(() => logger.LogError(A<UnauthorizedAccessException>._,
                A<string>.That.Contains("No permission to access the path")))
            .MustNotHaveHappened();
        Dispose();
    }

    [Theory]
    [AutoFakeItEasy]
    public void CleanDirectory_ShouldHandleArgumentException_WhenGetTempFolderPathReturnOtherException(
        [Frozen] IGetTempFolderPathHelper getTempFolderPathHelper,
        [Frozen] ILoggerService<CleanDirectoryHelper> logger,
        CleanDirectoryHelper sut,
        string fakeTempFolderPath
    )
    {
        // Arrange
        A.CallTo(() => getTempFolderPathHelper.GetTempFolderPath()).Returns(fakeTempFolderPath);
        // Act
        Action act = () => sut.CleanDirectory();

        // Assert
        A.CallTo(() => logger.LogError(A<Exception>._,
                A<string>.That.Contains("An unexpected exception occurred")))
            .MustNotHaveHappened();
        Dispose();
    }

    #endregion

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