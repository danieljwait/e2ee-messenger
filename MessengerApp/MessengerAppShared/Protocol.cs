using System;
using System.Collections.Generic;
using System.Text;

namespace MessengerAppShared
{
    public class Protocol
    {
        public string Text { get; set; }
        public byte[] Data { get; set; }

        // When created from a string
        public Protocol(string text)
        {
            Text = text;
            Data = Encoding.UTF8.GetBytes(text);
        }

        // When created from binary data
        public Protocol(byte[] data)
        {
            Text = Encoding.UTF8.GetString(data);
            Data = data;
        }
    }
}
