﻿using Caliburn.Micro;
using MessengerAppClient.Login.Messages;

namespace MessengerAppClient.Login.ViewModels
{
    public class SignupViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;

        public SignupViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public void BackToLoginButton()
        {
            // Tells Conductor to navigate back to LoginViewModel
            var InternalMessage = new LoginNavMessage(LoginPage.Login);
            _eventAggregator.PublishOnUIThread(InternalMessage);
        }
    }
}