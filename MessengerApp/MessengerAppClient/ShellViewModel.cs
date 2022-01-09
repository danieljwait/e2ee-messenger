using Caliburn.Micro;
using MessengerAppClient.Content.Message;
using MessengerAppClient.Content.ViewModels;
using MessengerAppClient.Login.Messages;
using MessengerAppClient.Login.ViewModels;

namespace MessengerAppClient
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

            Items.AddRange(new Screen[] { _loginConductorViewModel, _contentConductorViewModel });
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            _eventAggregator.Subscribe(this);
            ActivateItem(_loginConductorViewModel);
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            _eventAggregator.Unsubscribe(this);
        }

        public void Handle(ValidLoginMessage message)
        {
            ActivateItem(_contentConductorViewModel);
        }

        public void Handle(LogOutMessage message)
        {
            ActivateItem(_loginConductorViewModel);
        }
    }
}