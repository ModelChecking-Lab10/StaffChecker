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
}
