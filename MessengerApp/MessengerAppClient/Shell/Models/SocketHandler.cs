using MessengerAppShared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MessengerAppClient.Shell.Models
{
    public class SocketHandler : SocketBase
    {
        // List of currently connected users
        public List<string> ConnectedUsers = new List<string>();

        // Connect to server
        public void Connect()
        {
            // Connects to the server
            Socket.Connect(EndPoint);
        }

        // Send object to server
        public void SendToServer(object message)
        {
            // Serialises object into binary
            byte[] serialised = Protocol.Serialise(message);
            // Sends data {buffer, offset, size, flags}
            Socket.Send(serialised, 0, serialised.Length, SocketFlags.None);
        }

        // Begins a receive loop with a specified callback
        public void ReceiveLoop(Action<IAsyncResult> Callback)
        {
            Socket.BeginReceive(
                Buffer,                          // Buffer
                0,                               // Offset
                Buffer.Length,                   // Size
                SocketFlags.None,                // Flags
                new AsyncCallback(Callback),     // Callback
                Socket                           // State
                );
        }
    }
}
