using Caliburn.Micro;
using MessengerAppClient.Login.Messages;

namespace MessengerAppClient.Login.ViewModels
{
    public class LoginConductorViewModel : Conductor<Screen>.Collection.OneActive,
        IHandle<NavigateMessage>
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

        protected override void OnActivate()
        {
            base.OnActivate();
            _eventAggregator.Subscribe(this);
            ActivateItem(_loginViewModel);
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            _eventAggregator.Unsubscribe(this);
        }

        public void Handle(NavigateMessage message)
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