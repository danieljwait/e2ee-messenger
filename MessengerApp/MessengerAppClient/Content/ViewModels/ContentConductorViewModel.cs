using Caliburn.Micro;
using MessengerAppClient.Content.Message;

namespace MessengerAppClient.Content.ViewModels
{
    public class ContentConductorViewModel : Conductor<Screen>.Collection.OneActive,
        IHandle<NavigateMessage>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly HomeViewModel _homeViewModel;
        private readonly SettingsViewModel _settingsViewModel;

        public SideBarViewModel SideBar { get; }

        public ContentConductorViewModel(
            IEventAggregator eventAggregator,
            SideBarViewModel sideBarViewModel,
            HomeViewModel homeViewModel,
            SettingsViewModel settingsViewModel)
        {
            _eventAggregator = eventAggregator;
            SideBar = sideBarViewModel;
            _homeViewModel = homeViewModel;
            _settingsViewModel = settingsViewModel;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            _eventAggregator.Subscribe(this);
            ActivateItem(_homeViewModel);
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
                case ContentPage.Home:
                    ActivateItem(_homeViewModel);
                    break;
                case ContentPage.Settings:
                    ActivateItem(_settingsViewModel);
                    break;
                default:
                    break;
            }
        }
    }
}