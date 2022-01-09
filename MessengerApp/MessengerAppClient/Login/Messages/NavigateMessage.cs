namespace MessengerAppClient.Login.Messages
{
    public sealed class NavigateMessage
    {
        public LoginPage NavigateTo;

        public NavigateMessage(LoginPage navigateTo)
        {
            NavigateTo = navigateTo;
        }
    }

    public enum LoginPage
    {
        Login,
        Signup
    }
}