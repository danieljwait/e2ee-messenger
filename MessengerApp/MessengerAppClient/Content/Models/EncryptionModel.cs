using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MessengerAppClient.Content.Models
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
                    encrypted = csp.Encrypt(plaintext, false);
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
                    plaintext = csp.Decrypt(encrypted, false);
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
        public static Dictionary<string, string> RSAKeyGen(int bit_length = 1024)
        {
            Dictionary<string, string> key_pair = new Dictionary<string, string>();

            // 1024 bit key
            using (var csp = new RSACryptoServiceProvider(bit_length))
            {
                try
                {
                    // Gets public and private keys as XML
                    string public_key = csp.ToXmlString(false);
                    string private_key = csp.ToXmlString(true);

                    key_pair.Add("PublicKey", public_key);
                    key_pair.Add("PrivateKey", private_key);

                    return key_pair;
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
