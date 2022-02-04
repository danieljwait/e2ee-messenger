namespace MessengerAppClient.Shell.Messages
{
    public sealed class InternalClientMessage
    {
        public InternalClientCommand Command;
        public object Data;

        public InternalClientMessage(InternalClientCommand command_in, object data_in = null)
        {
            Command = command_in;
            Data = data_in;
        }
    }

    public enum InternalClientCommand
    {
        Connect,
        Disconnect,
        LogOut,
        ValidLogin,
        SendMessage,
        ClientConnect,
        ClientDisconnect,
        ReceiveClientList,
        LoginDetails,
        ReceiveMessage
    }
}
