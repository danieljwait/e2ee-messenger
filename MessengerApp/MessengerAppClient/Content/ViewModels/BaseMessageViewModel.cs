using MessengerAppShared.Models;

namespace MessengerAppClient.Content.ViewModels
{
    public class BaseMessageViewModel
    {
        private readonly MessageModel _message;

        public string Sender => _message.Sender;
        public string Recipient => _message.Recipient;
        public string Text => _message.Text;
        public string Time => _message.Time.ToShortTimeString();

        public BaseMessageViewModel(MessageModel message)
        {
            _message = message;
        }
    }
}
