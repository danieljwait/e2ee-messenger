using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;

namespace MessengerAppServer
{
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
