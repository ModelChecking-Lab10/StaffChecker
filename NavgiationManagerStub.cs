using Microsoft.AspNetCore.Components;

namespace StaffTesting.Tests.Helpers
{
    public class NavigationManagerStub : NavigationManager
    {
        public NavigationManagerStub()
        {
            // Initialize a fake base URI and current URI
            Initialize("http://localhost/", "http://localhost/");
        }

        // Keep track of the last navigation target for assertion
        public string? LastNavigatedTo { get; private set; }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            LastNavigatedTo = ToAbsoluteUri(uri).ToString();
        }
    }
}
