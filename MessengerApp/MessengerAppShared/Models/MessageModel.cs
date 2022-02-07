using System;

namespace MessengerAppShared.Models
{
    // Object for client to client messages
    [Serializable]
    public class MessageModel
    {
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
    }
}
