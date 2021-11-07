using System;
using System.Collections.Generic;
using System.Text;

namespace MessengerAppServer
{
    // Object for each record in credentials CSV
    public class Account
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}
