using Bunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using StaffClient.Components.Pages;
using StaffClient.Models;
using StaffClient.Services;
using Microsoft.AspNetCore.Components;

public class DeleteStaffTest : TestContext
{
    [Fact]
    public async Task ShouldDeleteStaffAndNavigate_WhenDeleteCalled()
    {
        // Arrange
        var mockService = new Mock<IStaffService>();
        var nav = new NavigationManagerStub();

        mockService.Setup(s => s.GetStaff(1))
                   .ReturnsAsync(new Staff { StaffId = 1, StaffName = "Bob" });

        Services.AddSingleton<IStaffService>(mockService.Object);
        Services.AddSingleton<NavigationManager>(nav);

        var cut = RenderComponent<StaffDetails>(parameters => parameters.Add(p => p.Id, "1"));

        // Act
        var baseComp = cut.Instance as StaffDetailsBase;
        await baseComp.DeleteStaff();

        // Assert
        mockService.Verify(s => s.DeleteStaff(1), Times.Once);
        Assert.Contains("/stafflist", nav.LastNavigatedTo);
    }

    [Fact]
    public async Task ShouldNotNavigate_WhenDeleteInvalidId()
    {
        // Arrange
        var mockService = new Mock<IStaffService>();
        var nav = new NavigationManagerStub();

        // Simulate DeleteStaff throwing (e.g., not found / 404)
        mockService.Setup(s => s.DeleteStaff(99))
                   .ThrowsAsync(new Exception("Staff not found"));

        Services.AddSingleton<IStaffService>(mockService.Object);
        Services.AddSingleton<NavigationManager>(nav);

        // Act
        var cut = RenderComponent<StaffDetails>(parameters => parameters.Add(p => p.Id, "99"));
        var baseComp = cut.Instance as StaffDetailsBase;

        var ex = await Record.ExceptionAsync(() => baseComp.DeleteStaff());

        // Assert
        Assert.NotNull(ex);
        Assert.Contains("Staff not found", ex.Message);
        Assert.Null(nav.LastNavigatedTo);
    }
}
