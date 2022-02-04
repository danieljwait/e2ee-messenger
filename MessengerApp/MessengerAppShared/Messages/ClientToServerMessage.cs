using System;

namespace MessengerAppShared.Messages
{
    // (-> Server) General message to server
    [Serializable]
    public sealed class ClientToServerMessage
    {
        public ClientCommand Command;
        public object Data;

        public ClientToServerMessage(ClientCommand command_in, object object_in = null)
        {
            Command = command_in;
            Data = object_in;
        }
    }

    [Serializable]
    public enum ClientCommand
    {
        Disconnect,
        LogOut,
        Login,
        Message
    }
}
