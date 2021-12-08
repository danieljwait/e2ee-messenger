using System;
using System.Collections.Generic;
using System.Text;

namespace MessengerAppShared
{
    // When a user logs in
    [Serializable]
    public class MessageLogin : MessageBase
    {
        public string Username;
        public string Password;

        public MessageLogin(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    // TODO: Call and handle MessageSend
    // When a user sends a message to another user
    [Serializable]
    public class MessageSend : MessageBase
    {
        public string Sender;
        public string Recipient;
        public string Text;

        public MessageSend(string sender, string recipient, string text)
        {
            Sender = sender;
            Recipient = recipient;
            Text = text;
        }
    }

    // Triggers pop-up for client (for debugging)
    [Serializable]
    public class MessageBoxResponse : MessageBase
    {
        public string Text;
        public string Caption;

        public MessageBoxResponse(string response, string caption)
        {
            Text = response;
            Caption = caption;
        }
    }

    // Echoes message (just for debugging)
    [Serializable]
    public class MessageEcho : MessageBase
    {
        public string Text;

        public MessageEcho(string text)
        {
            Text = text;
        }
    }

    // For sending an error message
    [Serializable]
    public class MessageError : MessageBase
    {
        public string Message;

        public MessageError(string message)
        {
            Message = message;
        }
    }
}
