using Caliburn.Micro;
using MessengerAppClient.Model;
using MessengerAppShared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


namespace MessengerAppClient.ViewModels
{
    class ShellViewModel : Screen
    {
        public ClientSocket socket = new ClientSocket();

        private string _messageToSend;
        private string _messageReceived;
        private string _connectionStatus = "Not connected";
        
        // TODO: Allow message to be sent with enter
        public string MessageToSend
        {
            get { return _messageToSend; }
            set { _messageToSend = value; }
        }

        // TODO: Replace single textblock with conversation
        public string MessageReceived
        {
            get { return _messageReceived; }
            set { _messageReceived = value; }
        }

        public string ConnectionStatus
        {
            get { return _connectionStatus; }
            set {
                _connectionStatus = value;

                // When the connection status changes, grey out the button
                NotifyOfPropertyChange(() => CanConnect);
            }
        }

        // Controls state of Connect button
        public bool CanConnect => (ConnectionStatus == "Not connected");

        // Connects program to the server
        public void Connect()
        {
            // Local socket starts connection with server
            socket.Connect();

            // Update UI
            ConnectionStatus = "Connected";
            NotifyOfPropertyChange(() => ConnectionStatus);

            // TODO: Infinite receive loop
        }

        // Sends a message to server and gets response
        public void SendMessage()
        {
            // Sends message to server
            socket.Send(MessageToSend, socket.Socket);
            // Gets server's response
            socket.Receive(socket.Socket);

            // Displays the received message (if available)
            if (socket.ReceiveMessages.Count != 0)
            {
                // Last message to be appended to list
                MessageReceived = socket.ReceiveMessages[^1];
                // Updates UI
                NotifyOfPropertyChange(() => MessageReceived);
            }
        }
    }
}
