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

        public void Connect()
        {
            Socket.Connect(EndPoint);
        }

        override public void Receive(Socket socket)
        {
            base.Receive(socket);
            waitHandle.WaitOne(); // Waits until the receive has finished before moving on
        }

        // TODO: Receive message from server
        public override void ReceiveCallback(IAsyncResult asyncResult)
        {
            // Gets socket from the async result
            Socket clientSocket = (Socket)asyncResult.AsyncState;
            // The amount of data received
            int received = clientSocket.EndReceive(asyncResult);

            byte[] dataBuffer = new byte[received];
            // Copy received bytes to data buffer
            Array.Copy(Buffer, dataBuffer, received);
            // Converts received byte[] to string
            string text = new Protocol(dataBuffer).Text;

            // Calls the functions to handle the message contents
            HandleMessage(clientSocket, text);

            // Sets flag that receive has ended
            waitHandle.Set();
        }

        public override void HandleMessage(Socket socket, string message)
        {
            // TODO: Change this to a queue
            ReceiveMessages.Add(message);
        }
    }
}
