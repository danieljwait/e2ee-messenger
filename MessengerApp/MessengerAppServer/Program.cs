using System;

namespace MessengerAppServer
{
    class Program
    {
        static void Main()
        {
            // Sets the title of the console's window
            Console.Title = "Messenger Server";

            // Creates a new server socket and starts it running
            ServerSocket serverSocket = new ServerSocket();
            serverSocket.Start();

            // User presses enter to close the server
            Console.ReadLine();
            serverSocket.Stop();
        }
    }
}
