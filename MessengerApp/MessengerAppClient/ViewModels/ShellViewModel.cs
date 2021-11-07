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
        public string MessageToSend
        {
            get { return _messageToSend; }
            set {
                _messageToSend = value;
                NotifyOfPropertyChange(() => MessageToSend);
            }
        }

        // TODO: Replace single TextBlock with a conversation type view
        private string _messageReceived;
        public string MessageReceived
        {
            get { return _messageReceived; }
            set {
                _messageReceived = value;
                NotifyOfPropertyChange(() => MessageReceived);
            }
        }

        private string _connectionStatus = "Not connected";
        public string ConnectionStatus
        {
            get { return _connectionStatus; }
            set {
                _connectionStatus = value;
                NotifyOfPropertyChange(() => ConnectionStatus);

                // When the connection status changes, checks whether to grey out the button
                NotifyOfPropertyChange(() => CanConnect);
            }
        }

        // Controls state of Connect button
        public bool CanConnect => (ConnectionStatus == "Not connected");

        // Connects to server
        public void Connect()
        {
            // Local socket starts connection with server
            socket.Connect();

            // Update UI
            ConnectionStatus = "Connected";
            // NotifyOfPropertyChange(() => ConnectionStatus);

            // TODO: Infinite receive loop
        }






        // TODO: Switch SendMessage to object based
        // Still uses text based networking

        // Send message to server, gets response
        public void SendMessage()
        {
            // Sends message to server
            socket.SendString(MessageToSend, socket.Socket);
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
