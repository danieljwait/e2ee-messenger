using Caliburn.Micro;
using MessengerAppClient.Login.Messages;

namespace MessengerAppClient.Login.ViewModels
{
    public class LoginConductorViewModel : Conductor<Screen>.Collection.OneActive,
        IHandle<LoginNavMessage>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly LoginViewModel _loginViewModel;
        private readonly SignupViewModel _signupViewModel;

        public LoginConductorViewModel(
            IEventAggregator eventAggregator,
            LoginViewModel loginViewModel,
            SignupViewModel signupViewModel)
        {
            _eventAggregator = eventAggregator;
            _loginViewModel = loginViewModel;
            _signupViewModel = signupViewModel;
        }

        // When instantiated, begin listening to EventAggregator and show LoginViewModel
        protected override void OnActivate()
        {
            base.OnActivate();
            _eventAggregator.Subscribe(this);
            ActivateItem(_loginViewModel);
        }

        // When deleted, stop listening to EventAggregator
        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            _eventAggregator.Unsubscribe(this);
        }

        // Determine which Screen to show using attribute of message
        public void Handle(LoginNavMessage message)
        {
            switch (message.NavigateTo)
            {
                case LoginPage.Login:
                    ActivateItem(_loginViewModel);
                    break;

                case LoginPage.Signup:
                    ActivateItem(_signupViewModel);
                    break;

                default:
                    break;
            }
        }
    }
}