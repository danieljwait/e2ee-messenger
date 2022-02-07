using System;
using System.Security.Cryptography;
using System.Text;


// TODO: Move back to CLIENT PROJECT


namespace MessengerAppServer
{
    public class EncryptionModel
    {
        // Allows encoding scheme to be changed centrally
        private static readonly Encoding _encoding = Encoding.UTF8;

        // Input plaintext string and XML key, output Base64 string
        public static string RSAEncrypt(string data, string key_info)
        {
            // Input string -> binary
            byte[] plaintext = _encoding.GetBytes(data);
            byte[] encrypted;

            using (var csp = new RSACryptoServiceProvider())
            {
                try
                {
                    // Imports contents of key information
                    csp.FromXmlString(key_info);

                    // Encrypts the data and pads
                    encrypted = csp.Encrypt(plaintext, true);
                }
                finally
                {
                    // Stops keys being stored in Windows
                    csp.PersistKeyInCsp = false;
                }
            }

            // Return Base64 string
            return Convert.ToBase64String(encrypted);
        }

        // Input Base64 string and XML key, output plaintext string
        public static string RSADecrypt(string data, string key_info)
        {
            // Input Base64 -> binary
            byte[] encrypted = Convert.FromBase64String(data);
            byte[] plaintext;

            using (var csp = new RSACryptoServiceProvider())
            {
                try
                {
                    // Imports contents of key information
                    csp.FromXmlString(key_info);

                    // Decrypts the data
                    plaintext = csp.Decrypt(encrypted, true);
                }
                finally
                {
                    // Stops keys being stored in Windows
                    csp.PersistKeyInCsp = false;
                }
            }

            // Return binary to string
            return _encoding.GetString(plaintext);
        }

        // Generates a key pair, returns XML representation of key pair
        public static string RSAKeyGen()
        {
            // 1024 bit key
            using (var csp = new RSACryptoServiceProvider(1024))
            {
                try
                {
                    // Gets public and private key pair as XML
                    string key = csp.ToXmlString(true);
                    return key;
                }
                finally
                {
                    // Stops keys being stored in Windows
                    csp.PersistKeyInCsp = false;
                }
            }
        }
    }
}
