using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;

namespace AutoFixtureTesting.Shared;

public class AutoFakeItEasyAttribute : AutoDataAttribute
{
    public AutoFakeItEasyAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoFixture.AutoFakeItEasy.AutoFakeItEasyCustomization());
        return fixture;
    }
}