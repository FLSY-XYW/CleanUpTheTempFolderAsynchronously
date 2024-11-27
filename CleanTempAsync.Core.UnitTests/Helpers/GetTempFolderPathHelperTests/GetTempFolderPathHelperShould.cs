using AutoFixture.Xunit2;
using AutoFixtureTesting.Shared;
using CleanTempAsync.Core.Exceptions;
using CleanTempAsync.Core.Helpers;
using CleanTempAsync.Core.Helpers.ImplementationClassLists;
using CleanTempAsync.Core.Helpers.InterfaceLists;
using FakeItEasy;
using FluentAssertions;
using Xunit.Abstractions;

namespace CleanTempAsync.Core.UnitTests.Helpers.GetTempFolderPathHelperTests;

public class GetTempFolderPathHelperShould
{
    private readonly ITestOutputHelper _testOutputHelper;

    public GetTempFolderPathHelperShould(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [AutoFakeItEasy]
    public void GetTempFolderPath_ShouldReturnTempPath_WhenCalled(
        [Frozen] ITempFolderPathProvider tempFolderPathProvider,
        GetTempFolderPathHelper sut
    )
    {
        // Arrange
        A.CallTo(() => tempFolderPathProvider.TempFolderPathProvider())
            .Returns(Path.GetDirectoryName(Path.GetTempFileName()));
        // Act
        string? actualResult = sut.GetTempFolderPath();
        // Assert
        actualResult.Should().NotBeNullOrEmpty();
        Directory.Exists(actualResult).Should().BeTrue();
    }

    [Theory]
    [AutoFakeItEasy]
    public void GetTempFolderPath_ShouldThrowException_WhenDirectoryNameIsNull(
        [Frozen] ITempFolderPathProvider tempFolderPathProvider,
        GetTempFolderPathHelper sut
    )
    {
        // Arrange
        A.CallTo(() => tempFolderPathProvider.TempFolderPathProvider()).Returns(null);
        // Act
        Action actualResult = () => sut.GetTempFolderPath();

        // Assert
        // act.Should().Throw<GetTempFolderPathHelperException>()
        //     .WithMessage("Could not get temp folder path");
        actualResult.Should().Throw<GetTempFolderPathHelperException>()
            .WithMessage("Could not get temp folder path");
    }

    [Theory]
    [AutoFakeItEasy]
    public void GetTempFolderPath_ShouldThrowException_WhenDirectoryNameIsNotExists(
        [Frozen] ITempFolderPathProvider tempFolderPathProvider,
        GetTempFolderPathHelper sut,
        string fakeTempFolderPath
    )
    {
        // Arrange
        A.CallTo(() => tempFolderPathProvider.TempFolderPathProvider()).Returns(fakeTempFolderPath);
        // Act
        Action actualResult = () => sut.GetTempFolderPath();

        // Assert
        // act.Should().Throw<GetTempFolderPathHelperException>()
        //     .WithMessage("Could not get temp folder path");
        actualResult.Should().Throw<GetTempFolderPathHelperException>()
            .WithMessage("The temp folder path does not exist");
    }
}