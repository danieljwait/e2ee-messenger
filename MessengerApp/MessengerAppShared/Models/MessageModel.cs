using System;

namespace MessengerAppShared.Models
{
    [Serializable]
    public class MessageModel
    {
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
    }
}
