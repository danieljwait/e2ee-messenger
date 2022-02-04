namespace MessengerAppClient.Content.Messages
{
    public sealed class ContentNavMessage
    {
        public ContentPage NavigateTo;

        public ContentNavMessage(ContentPage navigateTo)
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