namespace MessengerAppClient.Content.Message
{
    public sealed class NavigateMessage
    {
        public ContentPage NavigateTo;

        public NavigateMessage(ContentPage navigateTo)
        {
            NavigateTo = navigateTo;
        }
    }

    public enum ContentPage
    {
        Home,
        Settings
    }
}