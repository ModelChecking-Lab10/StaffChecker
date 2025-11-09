using Bunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using StaffClient.Components.Pages;
using StaffClient.Services;

public class AddStaffValidationTests : TestContext
{
    public AddStaffValidationTests()
    {
        var mockService = new Mock<IStaffService>();
        Services.AddSingleton<IStaffService>(mockService.Object);
    }

    [Fact]
    public void ShouldShowValidationMessages_ForInvalidEmailAndPhone()
    {
        // Arrange
        var cut = RenderComponent<AddStaff>();

        // Act — enter invalid values
        cut.Find("#staffName").Change("Tam La");
        cut.Find("#email").Change("tamla@");
        cut.Find("#phone").Change("12abc34");

        cut.Find("form").Submit();

        // Assert
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Invalid email format", cut.Markup);
            Assert.Contains("Invalid phone number format", cut.Markup);
        });
    }

    [Fact]
    public void ShouldNotShowValidationMessages_ForValidEmailAndPhone()
    {
        var cut = RenderComponent<AddStaff>();

        cut.Find("#staffName").Change("Tam La");
        cut.Find("#email").Change("tamla@example.com");
        cut.Find("#phone").Change("0123456789");

        cut.Find("form").Submit();

        Assert.DoesNotContain("Invalid email format", cut.Markup);
        Assert.DoesNotContain("Invalid phone number format", cut.Markup);
    }
}
