using Caliburn.Micro;
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

namespace MessengerAppClient.ViewModels
{
    class LoginViewModel : Screen
    {
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

        // When programs first starts up, create server connection
        public LoginViewModel()
        {
            // Connects to the server
            socket.Connect();
        }

        public void LoginButton()
        {
            // Create MessageLogin object using GUI fields
            MessageLogin login_request = new MessageLogin(UsernameInput, PasswordInput);

            // Sends the object to the server
            socket.SendObject(login_request, socket.Socket);

            // Clear the two input fields
            UsernameInput = "";
            PasswordInput = "";

            // Receives the response
            socket.ReceiveObject(socket.Socket);
        }

        // TODO: Implement Signup (not priority now)
        public void SignupButton()
        {
            MessageBox.Show("This button is under construction", "Create an account");
        }
    }
}
