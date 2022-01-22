using Caliburn.Micro;
using MessengerAppClient.Content.ViewModels;
using MessengerAppClient.Login.ViewModels;
using MessengerAppClient.Shell.Messages;

namespace MessengerAppClient.Shell.ViewModels
{
    public class ShellViewModel : Conductor<Screen>.Collection.OneActive,
        IHandle<ValidLoginMessage>, IHandle<LogOutMessage>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly LoginConductorViewModel _loginConductorViewModel;
        private readonly ContentConductorViewModel _contentConductorViewModel;

        public ShellViewModel(
            IEventAggregator eventAggregator,
            LoginConductorViewModel loginConductorViewModel,
            ContentConductorViewModel contentConductorViewModel)
        {
            _eventAggregator = eventAggregator;
            _loginConductorViewModel = loginConductorViewModel;
            _contentConductorViewModel = contentConductorViewModel;
        }

        // When instantiated, begin listening to EventAggregator and show LoginConductorViewModel
        protected override void OnActivate()
        {
            base.OnActivate();
            _eventAggregator.Subscribe(this);
            ActivateItem(_loginConductorViewModel);
        }

        // When deleted, stop listening to EventAggregator
        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            _eventAggregator.Unsubscribe(this);
        }

        // Activate Messaging Conductor
        // Deactivate Login Conductor
        public void Handle(ValidLoginMessage message)
        {
            ActivateItem(_contentConductorViewModel);
        }

        // Activate Login Conductor
        // Deactivate Messaging Conductor
        public void Handle(LogOutMessage message)
        {
            ActivateItem(_loginConductorViewModel);
        }
    }
}