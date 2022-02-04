using Caliburn.Micro;
using MessengerAppClient.Login.Messages;
using MessengerAppClient.Shell.Messages;
using System.Collections.Generic;
using MessengerAppShared.Messages;

namespace MessengerAppClient.Login.ViewModels
{
    public class LoginViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;

        // GUI input fields
        private string _usernameInput;
        private string _passwordInput;

        public string UsernameInput
        {
            get { return _usernameInput; }
            set
            {
                _usernameInput = value;
                NotifyOfPropertyChange(() => UsernameInput);
            }
        }

        public string PasswordInput
        {
            get { return _passwordInput; }
            set
            {
                _passwordInput = value;
                NotifyOfPropertyChange(() => PasswordInput);
            }
        }

        // When programs first starts up, create server connection
        public LoginViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        // Send login request when "login" button pressed
        public void LoginButton()
        {
            // Gets credentials from GUI fields
            var Credentials = new Dictionary<string, string>()
            {
                {"Username", UsernameInput },
                {"Password", PasswordInput }
            };

            // Sends the login request to server
            var ServerMessage = new ClientToServerMessage(ClientCommand.Login, Credentials);
            var InternalMessage = new InternalClientMessage(InternalClientCommand.SendMessage, ServerMessage);
            _eventAggregator.PublishOnUIThread(InternalMessage);

            // Clear the two input fields
            UsernameInput = "";
            PasswordInput = "";
        }

        // TODO: Implement Signup (not priority now)
        public void SignupButton()
        {
            // Tell Conductor to navigate to the SignupViewModel
            _eventAggregator.PublishOnUIThread(new LoginNavMessage(LoginPage.Signup));
        }
    }
}
