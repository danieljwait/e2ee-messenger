using System;

namespace MessengerAppServer
{
    public class Program
    {
        public static void Main()
        {
            // Sets the title of the console's window
            Console.Title = "MessengerApp Server";

            // Creates a new server socket and starts it running
            ServerSocket serverSocket = new ServerSocket();
            serverSocket.Start();

            // User presses enter to close the server
            _ = Console.ReadLine();
            serverSocket.Stop();

            // TODO: Fix errors when closing server by ending all client threads
        }
    }
}
