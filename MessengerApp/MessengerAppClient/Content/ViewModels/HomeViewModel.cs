using Caliburn.Micro;
using MessengerAppClient.Content.Models;
using MessengerAppClient.Shell.Messages;
using MessengerAppShared.Messages;
using MessengerAppShared.Models;
using System;
using System.Collections.Generic;

namespace MessengerAppClient.Content.ViewModels
{
    public class HomeViewModel : Screen,
        IHandle<InternalClientMessage>
    {
        // Event Aggregator instance
        private readonly IEventAggregator _eventAggregator;

        public HomeViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        // Collection of currently logged in users
        private BindableCollection<UserModel> _users = new BindableCollection<UserModel>();
        public BindableCollection<UserModel> Users
        {
            get { return _users; }
            set 
            {
                _users = value;
                NotifyOfPropertyChange(() => Users);
                NotifyOfPropertyChange(() => SelectedUser);
            }
        }

        // User that is selected in the combo box
        private UserModel _selectedUser;
        public UserModel SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                NotifyOfPropertyChange(() => SelectedUser);
            }
        }

        // Fields to input and display messages
        private string _messageToSend;
        public string MessageToSend
        {
            get { return _messageToSend; }
            set {
                _messageToSend = value;
                NotifyOfPropertyChange(() => MessageToSend);
            }
        }

        // Holds user's username
        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                NotifyOfPropertyChange(() => Username);
            }
        }

        // Send message to server
        public void SendMessage()
        {
            var message = new MessageModel()
            {
                Sender = Username,
                Recipient = SelectedUser.Username,
                Text = MessageToSend,
                Time = DateTime.Now
            };

            MessageToSend = "";

            // Add to own list of messages
            SelectedUser.Messages.Add(new OutboundMessageViewModel(message));

            // Send to recipient
            var ServerMessage = new ClientToServerMessage(ClientCommand.Message, message);
            var InternalMessage = new InternalClientMessage(InternalClientCommand.SendMessage, ServerMessage);
            _eventAggregator.PublishOnUIThread(InternalMessage);
        }

        // Receive list of clients from server and add to combo box
        private void ReceiveClientList(List<string> new_list)
        {
            Users.Clear();

            foreach (string username in new_list)
            {
                var messages = new BindableCollection<BaseMessageViewModel>();

                //Stops self being in list
                if (username != Username)
                {
                    Users.Add(new UserModel() { Username = username, Messages = messages });
                }
            }
        }

        // Update combo box to remove a user
        private void RemoveClient(string username)
        {
            foreach (UserModel user in Users)
            {
                if (user.Username == username)
                {
                    Users.Remove(user);
                }
            }
        }

        // Update combo box to add a user
        private void AddClient(string username)
        {
            // Stops self being in list
            if (username != Username)
            {
                Users.Add(new UserModel { Username = username, Messages = new BindableCollection<BaseMessageViewModel>()});
            }
        }

        // Store the credentials received from the server
        private void SetLoginDetails(Dictionary<string, string> credentials)
        {
            Username = credentials["Username"];
        }

        private void ReceiveMessage(MessageModel message)
        {
            foreach (UserModel User in Users)
            {
                if (User.Username == message.Sender)
                {
                    User.Messages.Add(new InboundMessageViewModel(message));
                }
            }
        }

        // Handle internal
        public void Handle(InternalClientMessage message)
        {
            // Switch on the Command enum in the message
            switch (message.Command)
            {
                case InternalClientCommand.LoginDetails:
                    SetLoginDetails((Dictionary<string, string>)message.Data);
                    break;

                case InternalClientCommand.ClientConnect:
                    AddClient((string)message.Data);
                    break;

                case InternalClientCommand.ClientDisconnect:
                    RemoveClient((string)message.Data);
                    break;

                case InternalClientCommand.ReceiveClientList:
                    ReceiveClientList((List<string>)message.Data);
                    break;

                case InternalClientCommand.ReceiveMessage:
                    ReceiveMessage((MessageModel)message.Data);
                    break;
            }
        }

        // When screen opened, begin listening to EventAggregator
        protected override void OnActivate()
        {
            base.OnActivate();
            _eventAggregator.Subscribe(this);
        }

        // When screen closed, stop listening to EventAggregator
        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            _eventAggregator.Unsubscribe(this);
        }
    }
}
