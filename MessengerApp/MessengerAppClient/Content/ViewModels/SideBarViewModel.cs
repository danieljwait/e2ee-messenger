using Caliburn.Micro;
using MessengerAppClient.Content.Messages;
using MessengerAppClient.Shell.Messages;
using MessengerAppShared.Models;

namespace MessengerAppClient.Content.ViewModels
{
    public class SideBarViewModel : Screen, IHandle<InternalClientMessage>
    {
        private readonly IEventAggregator _eventAggregator;

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

        public SideBarViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
        }

        // Tells Conductor to navigate to HomeViewModel
        public void NavigateHome()
        {
            var InternalMessage = new ContentNavMessage(ContentPage.Home);
            _eventAggregator.PublishOnUIThread(InternalMessage);
        }

        // Tells Conductor to navigate to SettingsViewModel
        public void NavigateSettings()
        {
            var InternalMessage = new ContentNavMessage(ContentPage.Settings);
            _eventAggregator.PublishOnUIThread(InternalMessage);
        }

        // Tells Conductor to navigate to LoginViewModel
        public void NavigateLogOut()
        {
            var InternalMessage = new InternalClientMessage(InternalClientCommand.LogOut);
            _eventAggregator.PublishOnUIThread(InternalMessage);
        }

        // Handle internal
        public void Handle(InternalClientMessage message)
        {
            // Switch on the Command enum in the message
            switch (message.Command)
            {
                case InternalClientCommand.LoginDetails:
                    var credentials = (AccountModel)message.Data;
                    Username = credentials.Username;
                    break;
            }
        }
    }
}
