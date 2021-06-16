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
        
        public string MessageToSend
        {
            get { return _messageToSend; }
            set { _messageToSend = value; }
        }

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

        public ShellViewModel()
        {
            // TODO: Wire-up buttons
            // TODO: Dynamic add TextBlocks to StackPanel
        }

        // Boolean to control state of Connect button
        public bool CanConnect => (ConnectionStatus == "Not connected");

        public void Connect()
        {
            socket.Connect();
            ConnectionStatus = "Connected";
            NotifyOfPropertyChange(() => ConnectionStatus);

            // TODO: Begins a loop to receive
        }

        public void SendMessage()
        {
            socket.Send(MessageToSend, socket.Socket);
            socket.Receive(socket.Socket);

            // Display the received message
            // TODO: Wait for ReceiveCallback return before running this
            if (socket.ReceiveMessages.Count != 0)
            {
                MessageReceived = socket.ReceiveMessages[socket.ReceiveMessages.Count - 1];
                NotifyOfPropertyChange(() => MessageReceived);
            }

        }
    }
}
