using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;

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

    // Object that interfaces with the CSV file
    public class CSVHandler
    {
        public static IEnumerable<Account> GetAccounts()
        {
            // Opens the CSV file from resources
            StringReader stringReader = new StringReader(Properties.Resources.AccountCredentials);
            // Reads the contents from the stream reader
            CsvReader csvReader = new CsvReader(stringReader, CultureInfo.CurrentCulture);
            // Gets all the records from the contents
            IEnumerable<Account> accounts = csvReader.GetRecords<Account>();

            return accounts;
        }
    }
}
