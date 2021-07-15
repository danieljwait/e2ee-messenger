using MessengerAppShared;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MessengerAppClient.Model
{
    public class ClientSocket : SocketBase
    {
        public List<string> ReceiveMessages = new List<string>();
        public AutoResetEvent waitHandle = new AutoResetEvent(false);

        // Connects the client's socket to the server
        public void Connect()
        {
            Socket.Connect(EndPoint);
        }

        // Receives data from the server, awaits completion
        public override void Receive(Socket socket)
        {
            // Minimal override needed so parent method is called first
            base.Receive(socket);
            // BaseSocket's receive is asynchronous, so must wait for reply before ending call
            waitHandle.WaitOne();
        }

        // Finishes receiving data from server, passes message to handler
        public override void ReceiveCallback(IAsyncResult asyncResult)
        {
            // Minimal override needed so parent method is called first
            base.ReceiveCallback(asyncResult);

            // Sets flag that receive has ended
            waitHandle.Set();
        }

        // Handles messages from the server
        public override void HandleMessage(Socket socket, string message)
        {
            // TODO: Change this to a queue
            ReceiveMessages.Add(message);
        }
    }
}
