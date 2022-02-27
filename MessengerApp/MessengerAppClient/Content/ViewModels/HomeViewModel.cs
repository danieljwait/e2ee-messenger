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
            set
            {
                _messageToSend = value;
                NotifyOfPropertyChange(() => MessageToSend);
            }
        }

        // Store keys for encryption
        private string _private_key;

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
            // Message to show on this client's screen
            var standard_message = new MessageModel()
            {
                Sender = Username,
                Recipient = SelectedUser.Username,
                Text = MessageToSend,
                Time = DateTime.Now
            };

            // Clear message UI field
            MessageToSend = "";

            // Add to own list of messages
            SelectedUser.Messages.Add(new OutboundMessageViewModel(standard_message));

            // Encrypted message, remaking object avoids reference issues after encryption
            var encrypted_message = new MessageModel()
            {
                Sender = standard_message.Sender,
                Recipient = standard_message.Recipient,
                Text = EncryptionModel.RSAEncrypt(standard_message.Text, SelectedUser.PublicKey),
                Time = standard_message.Time
            };

            // Send to recipient
            var ServerMessage = new ClientToServerMessage(ClientCommand.Message, encrypted_message);
            var InternalMessage = new InternalClientMessage(InternalClientCommand.SendMessage, ServerMessage);
            _eventAggregator.PublishOnUIThread(InternalMessage);
        }

        // Receiving a message
        private void ReceiveMessage(MessageModel message)
        {
            // Find recipient's key
            foreach (UserModel User in Users)
            {
                if (User.Username == message.Sender)
                {
                    // Decrypt message
                    message.Text = EncryptionModel.RSADecrypt(message.Text, _private_key);

                    User.Messages.Add(new InboundMessageViewModel(message));

                    return;
                }
            }
        }

        // Receive list of clients from server and add to combo box
        private void ReceiveClientList(List<AccountModel> credentials_list)
        {
            Users.Clear();

            foreach (AccountModel credentials in credentials_list)
            {
                //Stops self being in list
                if (credentials.Username != Username)
                {
                    AddClient(credentials);
                }
            }
        }

        // Update combo box to remove a user
        private void RemoveClient(string username)
        {
            // Error from deleting element in Users while iterating through Users
            try
            {
                foreach (UserModel user in Users)
                {
                    if (user.Username == username)
                    {
                        Users.Remove(user);
                    }
                }
            }
            catch { }
        }

        // Update combo box to add a user
        private void AddClient(AccountModel credentials)
        {
            // Stops self being in list
            if (credentials.Username != Username)
            {
                var new_user = new UserModel()
                {
                    Username = credentials.Username,
                    PublicKey = credentials.PublicKey,
                    Messages = new BindableCollection<BaseMessageViewModel>()
                };

                Users.Add(new_user);
            }
        }

        // Store the credentials received from the server
        private void SetLoginDetails(AccountModel credentials)
        {
            Username = credentials.Username;
            _private_key = credentials.PrivateKey;

            foreach (UserModel user in Users)
            {
                // Remove connections to self from user list
                if (user.Username == Username)
                {
                    Users.Remove(user);
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
                    SetLoginDetails((AccountModel)message.Data);
                    break;

                case InternalClientCommand.ClientConnect:
                    AddClient((AccountModel)message.Data);
                    break;

                case InternalClientCommand.ClientDisconnect:
                    RemoveClient((string)message.Data);
                    break;

                case InternalClientCommand.ReceiveClientList:
                    ReceiveClientList((List<AccountModel>)message.Data);
                    break;

                case InternalClientCommand.ReceiveMessage:
                    ReceiveMessage((MessageModel)message.Data);
                    break;

                case InternalClientCommand.LogOut:
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
