using Caliburn.Micro;
using MessengerAppClient.Content.Message;

namespace MessengerAppClient.Content.ViewModels
{
    public class SideBarViewModel
    {
        private readonly IEventAggregator _eventAggregator;

        public SideBarViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public void NavigateHome()
        {
            _eventAggregator.PublishOnUIThread(new NavigateMessage(ContentPage.Home));
        }

        public void NavigateSettings()
        {
            _eventAggregator.PublishOnUIThread(new NavigateMessage(ContentPage.Settings));
        }

        public void NavigateLogOut()
        {
            _eventAggregator.PublishOnUIThread(new LogOutMessage());
        }
    }
}
