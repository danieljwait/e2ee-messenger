using Caliburn.Micro;

namespace MessengerAppClient.Content.ViewModels
{
    public class SettingsViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;

        public SettingsViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }
    }
}
