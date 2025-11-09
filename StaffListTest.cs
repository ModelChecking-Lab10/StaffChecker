using Bunit;
using Moq;
using StaffClient.Components.Pages;
using StaffClient.Models;
using StaffClient.Services;
using Microsoft.Extensions.DependencyInjection;

public class StaffListTests : TestContext
{
    [Fact]
    public void ShouldRenderSpinner_WhenStaffsNull()
    {
        var mockService = new Mock<IStaffService>();
        mockService.Setup(s => s.GetStaffs())
                   .ReturnsAsync(new List<Staff>());
        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<StaffList>();
        cut.WaitForState(() => cut.Markup.Contains("spinner"));

        Assert.Contains("spinner", cut.Markup);
    }

    [Fact]
    public void ShouldRenderCards_WhenStaffsAvailable()
    {
        var mockService = new Mock<IStaffService>();
        mockService.Setup(s => s.GetStaffs()).ReturnsAsync(new List<Staff>
        {
            new Staff { StaffId = 1, StaffName = "Alice" },
            new Staff { StaffId = 2, StaffName = "Bob" }
        });
        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<StaffList>();

        cut.WaitForState(() => cut.FindAll(".card").Count == 2);

        var cards = cut.FindAll(".card");
        Assert.Equal(2, cards.Count);
        Assert.Contains("Alice", cut.Markup);
        Assert.Contains("Bob", cut.Markup);
    }
}
