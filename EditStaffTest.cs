using Bunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using StaffClient.Components.Pages;
using StaffClient.Models;
using StaffClient.Services;
using Microsoft.AspNetCore.Components;

public class EditStaffTest : TestContext
{
    [Fact]
    public async Task ShouldCallUpdateStaffAndNavigate_WhenFormValid()
    {
        // Arrange
        var mockService = new Mock<IStaffService>();
        var nav = new NavigationManagerStub();

        mockService.Setup(s => s.GetStaff(It.IsAny<int>()))
                   .ReturnsAsync(new Staff
                   {
                       StaffId = 1,
                       StaffName = "Alice",
                       Email = "alice@example.com",
                       PhoneNumber = "0987654321",
                       StartingDate = DateTime.Now
                   });

        Services.AddSingleton<IStaffService>(mockService.Object);
        Services.AddSingleton<NavigationManager>(nav);

        var cut = RenderComponent<EditStaff>(parameters => parameters.Add(p => p.Id, "1"));

        // Act
        var baseComp = cut.Instance as EditStaffBase;
        baseComp.staff.StaffName = "Alice Updated";
        await baseComp.HandleValidSubmit();

        // Assert
        mockService.Verify(s => s.UpdateStaff(1, It.Is<Staff>(st => st.StaffName == "Alice Updated")), Times.Once);
        Assert.Contains("/stafflist", nav.LastNavigatedTo);
    }

    [Fact]
    public async Task ShouldShowValidationMessages_WhenFormInvalid()
    {
        var mockService = new Mock<IStaffService>();
        var nav = new NavigationManagerStub();

        mockService.Setup(s => s.GetStaff(1))
            .ReturnsAsync(new Staff
            {
                StaffId = 1,
                StaffName = "John",
                Email = "john@example.com",
                PhoneNumber = "0987654321",
                StartingDate = DateTime.Now
            });

        Services.AddSingleton<IStaffService>(mockService.Object);
        Services.AddSingleton<NavigationManager>(nav);

        var cut = RenderComponent<EditStaff>(parameters => parameters.Add(p => p.Id, "1"));

        // Wait until component fully rendered (staff loaded)
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Email", cut.Markup);
        });

        // Find fresh inputs AFTER render
        cut.Find("#email").Change("not-an-email");
        cut.Find("#phone").Change("abc123");

        // Submit form
        cut.Find("form").Submit();

        // Validate error messages
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Invalid email format", cut.Markup);
            Assert.Contains("Invalid phone number format", cut.Markup);
        });

        // Ensure no update and no navigation
        mockService.Verify(s => s.UpdateStaff(It.IsAny<int>(), It.IsAny<Staff>()), Times.Never);
        Assert.Null(nav.LastNavigatedTo);
    }
}
