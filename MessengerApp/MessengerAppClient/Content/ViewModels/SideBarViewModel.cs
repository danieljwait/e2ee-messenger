using Caliburn.Micro;
using MessengerAppClient.Content.Messages;
using MessengerAppClient.Shell.Messages;

namespace MessengerAppClient.Content.ViewModels
{
    public class SideBarViewModel
    {
        private readonly IEventAggregator _eventAggregator;

        public SideBarViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        // Tells Conductor to navigate to HomeViewModel
        public void NavigateHome()
        {
            _eventAggregator.PublishOnUIThread(new NavigateMessage(ContentPage.Home));
        }

        // Tells Conductor to navigate to SettingsViewModel
        public void NavigateSettings()
        {
            _eventAggregator.PublishOnUIThread(new NavigateMessage(ContentPage.Settings));
        }

        // Tells Conductor to navigate to LoginViewModel
        // Need to add functionality to sign out from server too
        public void NavigateLogOut()
        {
            _eventAggregator.PublishOnUIThread(new LogOutMessage());
        }
    }
}
