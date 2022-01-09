using Caliburn.Micro;
using MessengerAppClient.Login.Messages;
using MessengerAppClient.Model;
using MessengerAppShared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

// https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.page?view=windowsdesktop-5.0
// https://docs.microsoft.com/en-us/dotnet/api/system.windows.window.showdialog?redirectedfrom=MSDN&view=windowsdesktop-5.0#System_Windows_Window_ShowDialog
// https://stackoverflow.com/questions/499294/how-to-make-modal-dialog-in-wpf
// https://stackoverflow.com/questions/11499932/wpf-popup-window

namespace MessengerAppClient.Login.ViewModels
{
    public class LoginViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;

        /*
        public ClientSocket socket = new ClientSocket();

        private string _usernameInput;
        public string UsernameInput
        {
            get { return _usernameInput; }
            set
            {
                _usernameInput = value;
                NotifyOfPropertyChange(() => UsernameInput);
            }
        }

        private string _passwordInput;
        public string PasswordInput
        {
            get { return _passwordInput; }
            set
            {
                _passwordInput = value;
                NotifyOfPropertyChange(() => PasswordInput);
            }
        }
        /**/

        // When programs first starts up, create server connection
        public LoginViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            // Connects to the server
            // socket.Connect();
        }

        public void LoginButton()
        {
            /*
            // Create MessageLogin object using GUI fields
             MessageLogin login_request = new MessageLogin(UsernameInput, PasswordInput);

            // Sends the object to the server
            socket.SendObject(login_request, socket.Socket);

            // Clear the two input fields
            UsernameInput = "";
            PasswordInput = "";

            // Receives the response
            socket.ReceiveObject(socket.Socket);
            /**/

            _eventAggregator.PublishOnUIThread(new ValidLoginMessage());
        }

        // TODO: Implement Signup (not priority now)
        public void SignupButton()
        {
            _eventAggregator.PublishOnUIThread(new NavigateMessage(LoginPage.Signup));
        }
    }
}
