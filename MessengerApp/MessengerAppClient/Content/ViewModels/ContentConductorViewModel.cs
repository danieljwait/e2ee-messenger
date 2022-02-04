using Caliburn.Micro;
using MessengerAppClient.Content.Messages;

namespace MessengerAppClient.Content.ViewModels
{
    public class ContentConductorViewModel : Conductor<Screen>.Collection.OneActive,
        IHandle<ContentNavMessage>
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

        // When instantiated, begin listening to EventAggregator and show HomeViewModel
        protected override void OnActivate()
        {
            base.OnActivate();
            _eventAggregator.Subscribe(this);
            ActivateItem(_homeViewModel);
        }

        // When deleted, stop listening to EventAggregator
        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            _eventAggregator.Unsubscribe(this);
        }

        // Determine which Screen to show using attribute of message
        public void Handle(ContentNavMessage message)
        {
            switch (message.NavigateTo)
            {
                case ContentPage.Home:
                    ActivateItem(_homeViewModel);
                    break;

                case ContentPage.Settings:
                    ActivateItem(_settingsViewModel);
                    break;
            }
        }
    }
}