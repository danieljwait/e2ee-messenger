using System;

namespace MessengerAppShared.Models
{
    // Object for each record in credentials CSV
    [Serializable]
    public class AccountModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}
