using Caliburn.Micro;
using MessengerAppClient.Content.ViewModels;

namespace MessengerAppClient.Content.Models
{
    public class UserModel
    {
        public string Username { get; set; }
        public BindableCollection<BaseMessageViewModel> Messages { get; set; }
    }
}
