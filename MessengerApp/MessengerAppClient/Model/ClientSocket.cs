using MessengerAppShared;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace MessengerAppClient.Model
{
    public class ClientSocket : SocketBase
    {
        public List<string> ReceiveMessages = new List<string>();  // TODO: save list of objects instead, change to queue
        public AutoResetEvent waitHandle = new AutoResetEvent(false);  // TODO: Implement blocking receive for object networking

        // Connect client to server
        public void Connect()
        {
            Socket.Connect(EndPoint);
        }

        public override void HandleObject(Socket socket, object received_object)
        {
            // Each case is a different message type, since type determines purpose
            switch (received_object)
            {
                case MessageBoxResponse box_response:
                    Console.WriteLine("Triggered: MessageBoxResponse");
                    HandleObject_BoxResponse(box_response);
                    break;

                default:
                    Console.WriteLine("Triggered: Type unknown");
                    break;
            }
        }

        public void HandleObject_BoxResponse(MessageBoxResponse messageBoxResponse)
        {
            // Displays message from server as pop-up box
            MessageBox.Show(messageBoxResponse.Text, messageBoxResponse.Caption);

            // Call function to close LoginViewModel and open ShellViewModel
        }





        // *** Old text based networking (still in ShellViewModel) ***

        // Start receive from server, awaits completion
        public override void Receive(Socket socket)
        {
            // Minimal override needed so parent method is called first
            base.Receive(socket);
            // BaseSocket's receive is asynchronous, so must wait for reply before returning
            waitHandle.WaitOne();
        }

        // End receive from server, passes message to handler
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
            ReceiveMessages.Add(message);
        }
    }
}
