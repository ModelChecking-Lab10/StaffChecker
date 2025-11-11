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

    [Theory]
    [InlineData("Tam La", "tamla@", "12abc34")]
    [InlineData("Tam La", "tamlaexample.com", "phone123")]
    [InlineData("Tam La", "tamla@.com", "123-456-7890")]
    [InlineData("Tam La", "tamla", "(123)4567890")]
    [InlineData("Tam La", "tamla@com", "123.456.7890")]
    public void ShouldShowValidationMessages_ForInvalidEmailAndPhone(string name, string email, string phone)
    {
        // Arrange
        var cut = RenderComponent<AddStaff>();

        // Act
        cut.Find("#staffName").Change(name);
        cut.Find("#email").Change(email);
        cut.Find("#phone").Change(phone);

        cut.Find("form").Submit();

        // Assert
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Invalid email format", cut.Markup);
            Assert.Contains("Invalid phone number format", cut.Markup);
        });
    }

    [Theory]
    [InlineData("Tam La", "tamla@ctu.edu.vn", "0123456789")]
    [InlineData("Tam La", "tamla@gmail.com", "+84 987654321")]
    [InlineData("Tam La", "tamla@example.com", "+123 1234567890")]
    public void ShouldNotShowValidationMessages_ForValidEmailAndPhone(string name, string email, string phone)
    {
        var cut = RenderComponent<AddStaff>();

        cut.Find("#staffName").Change(name);
        cut.Find("#email").Change(email);
        cut.Find("#phone").Change(phone);

        cut.Find("form").Submit();

        Assert.DoesNotContain("Invalid email format", cut.Markup);
        Assert.DoesNotContain("Invalid phone number format", cut.Markup);
    }

    [Fact]
    public void ShouldShowValidationMessages_WhenFieldsAreEmpty()
    {
        // Arrange
        var cut = RenderComponent<AddStaff>();

        // Act
        cut.Find("form").Submit();

        // Assert
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("name is required", cut.Markup);
            Assert.Contains("Email is required", cut.Markup);
            Assert.Contains("Phone number is required", cut.Markup);
        });
    }
}
