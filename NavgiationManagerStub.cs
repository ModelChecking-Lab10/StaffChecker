using Microsoft.AspNetCore.Components;

public class NavigationManagerStub : NavigationManager
{
    public NavigationManagerStub()
    {
        Initialize("http://localhost/", "http://localhost/");
    }

    public string? LastNavigatedTo { get; private set; }

    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        LastNavigatedTo = ToAbsoluteUri(uri).ToString();
    }
}
