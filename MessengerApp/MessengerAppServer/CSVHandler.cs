using CsvHelper;
using MessengerAppShared.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MessengerAppServer
{
    // Object that interfaces with the CSV file
    public static class CSVHandler
    {
        // Returns objects for all users in CSV
        public static IEnumerable<AccountModel> GetAllAccounts()
        {
            // Opens the CSV file from resources
            StringReader stringReader = new StringReader(Properties.Resources.AccountCredentials);
            // Reads the contents from the stream reader
            CsvReader csvReader = new CsvReader(stringReader, CultureInfo.CurrentCulture);
            // Gets all the records from the contents
            IEnumerable<AccountModel> accounts = csvReader.GetRecords<AccountModel>();

            return accounts;
        }

        // Returns account with username and public key for specified username
        public static AccountModel GetUserPublicCredentials(string username)
        {
            var account_full = GetAccount(username);

            var account_public = new AccountModel()
            {
                Username = account_full.Username,
                PublicKey = account_full.PublicKey
            };

            return account_public;
        }

        // Returns account with username, public key and private key for specified username
        public static AccountModel GetUserPrivateCredentials(string username)
        {
            var account_full = GetAccount(username);

            var account_private = new AccountModel()
            {
                Username = account_full.Username,
                PublicKey = account_full.PublicKey,
                PrivateKey = account_full.PrivateKey
            };

            return account_private;
        }


        // Get account from CSV with specified name
        private static AccountModel GetAccount(string username)
        {
            // Query to get the account of the user with specified username
            var user_account = (from account in GetAllAccounts()
                                where account.Username == username
                                select account).FirstOrDefault();

            return user_account;
        }

        // Gets the public credentials (username, public key) of several users
        public static List<AccountModel> GetMutiPublicCredentials(List<string> users)
        {
            var all_credentials = new List<AccountModel>();

            foreach (var username in users)
            {
                all_credentials.Add(GetUserPublicCredentials(username));
            }

            return all_credentials;
        }
    }
}
