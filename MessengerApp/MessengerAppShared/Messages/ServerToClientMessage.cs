using System;

namespace MessengerAppShared.Messages
{
    // (-> Client) General message to client
    [Serializable]
    public sealed class ServerToClientMessage
    {
        public ServerCommand Command;
        public object Data;

        public ServerToClientMessage(ServerCommand command_in, object object_in = null)
        {
            Command = command_in;
            Data = object_in;
        }
    }

    [Serializable]
    public enum ServerCommand
    {
        LoginResult,
        ClientConnect,
        ClientDisconnect,
        SendClientsList,
        Message
    }
}
