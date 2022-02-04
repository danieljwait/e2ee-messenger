namespace MessengerAppClient.Login.Messages
{
    public sealed class LoginNavMessage
    {
        public LoginPage NavigateTo;

        public LoginNavMessage(LoginPage navigateTo)
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