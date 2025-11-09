using Bunit;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using StaffClient.Components.Pages;
using StaffClient.Models;
using StaffClient.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

public class StaffListTests : TestContext
{
    [Fact]
    public void ShouldRenderSpinner_WhenLoading()
    {
        // Arrange
        var mockService = new Mock<IStaffService>();

        // Simulate network delay so spinner renders before data returns
        mockService.Setup(s => s.GetStaffs()).Returns(async () =>
        {
            await Task.Delay(50);
            return new List<Staff>(); // after delay, return empty list
        });

        Services.AddSingleton<IStaffService>(mockService.Object);

        // Act
        var cut = RenderComponent<StaffList>();

        // Assert: spinner should be visible in initial render
        Assert.Contains("spinner", cut.Markup);
    }

    [Fact]
    public void ShouldRenderCards_WhenStaffsAvailable()
    {
        // Arrange
        var mockService = new Mock<IStaffService>();
        mockService.Setup(s => s.GetStaffs()).ReturnsAsync(new List<Staff>
        {
            new Staff { StaffId = 1, StaffName = "Alice" },
            new Staff { StaffId = 2, StaffName = "Bob" }
        });

        Services.AddSingleton<IStaffService>(mockService.Object);

        // Act
        var cut = RenderComponent<StaffList>();

        // Assert: Wait until async render finishes and cards appear
        cut.WaitForAssertion(() =>
        {
            var cards = cut.FindAll(".card");
            Assert.Equal(2, cards.Count);
            Assert.Contains("Alice", cut.Markup);
            Assert.Contains("Bob", cut.Markup);
        });
    }
}
