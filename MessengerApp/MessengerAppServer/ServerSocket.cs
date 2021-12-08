using MessengerAppShared;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MessengerAppServer
{
    public class ServerSocket : SocketBase
    {
        // Dictionaries of identifier:socket relations (and vice versa)
        public Dictionary<string, Socket> ClientSockets = new Dictionary<string, Socket>();
        public Dictionary<Socket, string> ClientSocketsInverse = new Dictionary<Socket, string>();

        // Maximum length of the connection queue for new clients
        public const int BACKLOG = 5;

        // Start socket listening on a port, begins accept client loop
        public void Start()
        {
            PrintMessage("Starting server...");

            // Socket binds to a port on the computer
            Socket.Bind(EndPoint);

            // Socket starts listening for data from the port
            // BACKLOG is the maximum length of the connection queue
            Socket.Listen(BACKLOG);

            PrintMessage("Server started");

            // Start of infinite accepting clients loop
            Socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        // Disconnect socket to end server
        public void Stop()
        {
            PrintMessage("Stopping server...");

            // Try used in case errors are thrown
            try
            {
                // Disables sending and receiving from the socket
                Socket.Shutdown(SocketShutdown.Both);
                // Closes the socket and releases all its resources
                Socket.Close();
            }
            // TODO: Find cause of exception causing clients to disconnect
            catch (Exception e) { PrintMessage("Error: " + e.Message); }

            PrintMessage("Server stopped");
        }

        // Accepts incoming client connection
        public void AcceptCallback(IAsyncResult asyncResult)
        {
            // Creates a socket for the new client to handle the connection
            Socket clientSocket = Socket.EndAccept(asyncResult);
            // Adds the new client's info to dictionaries indexing name:socket relations
            ClientSockets.Add(clientSocket.RemoteEndPoint.ToString(), clientSocket);
            ClientSocketsInverse.Add(clientSocket, clientSocket.RemoteEndPoint.ToString());

            PrintMessage($"{clientSocket.RemoteEndPoint} connected");

            // Starts listening for data from the new client
            ReceiveObject(clientSocket);

            // Starts accepting the next client (continues infinite loop)
            Socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        public override void HandleObject(Socket socket, object received_object)
        {
            // Each case is a different message type, since object type determines purpose
            switch (received_object)
            {
                case MessageLogin login:
                    HandleObject_Login(socket, login);
                    break;

                case MessageSend send:
                    HandleObject_Send(socket, send);  // TODO: Implement
                    break;

                case MessageEcho echo:  // Only called in testing
                    HandleObject_Echo(socket, echo);
                    break;

                case MessageError error:
                    HandleObject_Error(socket, error);
                    break;

                default:
                    HandleObject_Invalid(socket);  // TODO: Implement
                    break;
            }
        }

        public void HandleObject_Login(Socket socket, MessageLogin messageLogin)
        {
            MessageBoxResponse response;
            Account user_account;
            IEnumerable<Account> accounts = CSVHandler.GetAccounts();
            bool logged_in = false;

            // Compares the username and password against all of those in the CSV
            foreach (Account account in accounts)
            {
                // When there is a match, set login flag and break loop
                if (account.Username == messageLogin.Username && account.Password == messageLogin.Password)
                {
                    logged_in = true;
                    user_account = account;
                }
            }

            // Successful login
            if (logged_in)
            {
                response = new MessageBoxResponse($"Successful login. Welcome, {messageLogin.Username}!" +
                    "\nPassage to the messaging screen is currently unimplemented","Login attempt");
                PrintMessage($"{socket.RemoteEndPoint} logged in as '{messageLogin.Username}'");
            }
            // Unsuccessful login
            else
            {
                response = new MessageBoxResponse($"Unsuccessful login, try again", "Login attempt");
                PrintMessage($"{socket.RemoteEndPoint} attempted login to '{messageLogin.Username}'");
            }

            // Sends the MessageBoxResponse to client
            SendObject(response, socket);
        }

        public void HandleObject_Send(Socket socket, MessageSend messageSend) {
            // Get the text to be sent
            // Determine IP of recipient
            // Determine Name of sender
            // Make Object with message, sender, recipient
            // Send to recipient
            // Send confirmation to sender (?)
        }
        
        public void HandleObject_Echo(Socket socket, MessageEcho messageEcho) {
            // Creates new MessageEcho object containing the same text
            MessageBoxResponse echo = new MessageBoxResponse($"'{messageEcho.Text}'", "Echo");

            PrintMessage($"IN   {socket.RemoteEndPoint} '{messageEcho.Text}'");
            PrintMessage($"OUT  {socket.RemoteEndPoint} '{messageEcho.Text}'");

            // Sends the MessageBoxResponse to client
            SendObject(echo, socket);
        }

        public void HandleObject_Error(Socket socket, MessageError messageError)
        {
            // Outputs the error message to the server ouput
            PrintMessage($"{socket.RemoteEndPoint}'s error: {messageError.Message}");

            // Could confirm error receive || remove the client for safety
        }

        public void HandleObject_Invalid(Socket socket)
        {
            PrintMessage($"{socket.RemoteEndPoint} sent an invalid message");

            // TODO: Send client a MessageError saying they sent a bad message
        }





        // *** Old text based networking, still in ShellViewModel ***

        // Finishes receiving data from client, passes message to handler
        public override void ReceiveCallback(IAsyncResult asyncResult)
        {
            // Recreates client socket to handle connection
            Socket clientSocket = (Socket)asyncResult.AsyncState;

            // Stops infinite receive loop when client disconnects
            if (!SocketBase.IsConnected(clientSocket))
            {
                PrintMessage($"Client {ClientSocketsInverse[clientSocket]} has disconnected");

                // Removes client from dictionaries of connected clients
                ClientSockets.Remove(ClientSocketsInverse[clientSocket]);
                ClientSocketsInverse.Remove(clientSocket);

                return;
            }

            // Parent method called to finish receiving data and call handler
            base.ReceiveCallback(asyncResult);

            // Continues infinite receive loop
            Receive(clientSocket);
        }

        // Handles message from client
        public override void HandleMessage(Socket senderSocket, string message)
        {
            string response;
            // Removes trailing and leading whitespace
            message = message.Trim();
            // Split the messaging into words
            string[] parts = message.Split(' ');

            // Output debugging message to console: message received from client
            PrintMessage($"From {senderSocket.RemoteEndPoint} - {message}");

            // Switch on the first word (command) of message
            // ?? causes switch on empty string if LHS is null
            switch (parts[0].ToUpper() ?? String.Empty)
            {
                case "ECHO":
                    response = Command_ECHO(parts);
                    break;

                case "SEND":
                    response = Command_SEND(parts);
                    break;

                case "LOGIN":
                    response = Command_LOGIN(parts);
                    break;

                // When there is no command
                case "":
                    response = "Invalid message: Is null Or whitespace";
                    break;

                // When no matches have been made
                default:
                    response = $"Invalid command: Command \"{parts[0]}\" not found";
                    break;
            }

            // Output debugging message to console: what the client is being sent
            PrintMessage($"To   {senderSocket.RemoteEndPoint} - {response}");
            // Sends the message to the client
            SendString(response, senderSocket);
        }

        // Format: ECHO [message]
        private string Command_ECHO(string[] message)
        {
            string response = "Invalid ECHO: Unknown error";

            // If there is no [message] parameter, send error to client
            if (message.Length <= 1)
            {
                response = "Invalid ECHO: Missing argument [message]";
            }
            // If there is a [message] parameter, process message
            else
            {
                // Gets all of [message] to be echoed back
                string arg_message = string.Join(' ', message, 1, message.Length - 1);

                // If [message] is whitespace, send error to client
                if (string.IsNullOrWhiteSpace(arg_message))
                {
                    response = "Invalid ECHO: [message] IsNullOrWhiteSpace";
                }
                // If [message] is not whitespace, echo message to client
                else
                {
                    response = arg_message;
                }
            }

            return response;
        }

        // Format: SEND [recipient] [message]
        private string Command_SEND(string[] message)
        {
            string response = "Invalid SEND: Unknown error";

            // If there are no [recipient] and [message] parameters, send error to client
            if (message.Length == 1)
            {
                response = "Invalid SEND: Missing arguments [recipient] and [message]";
            }
            // If just [recipient], send appropriate error to client
            else if (message.Length == 2)
            {
                // Gets [recipient] parameter
                string arg_recipient = message[1];

                // If [recipient] is in dictionary, tell client that just message is missing
                if (ClientSockets.ContainsKey(arg_recipient))
                {
                    response = "Invalid SEND: Missing argument [message]";
                }
                // If [recipient] is not in dictionary, tell client recipient is invalid and message is missing
                else
                {
                    response = $"Invalid SEND: Invalid [recipient] \"{arg_recipient}\" and missing [message]";
                }
            }
            // If [recipient] and [message]
            else if (message.Length >= 3)
            {
                // Gets [recipient]
                string arg_recipient = message[1];

                // If [recipient] is valid
                if (ClientSockets.ContainsKey(arg_recipient))
                {
                    // Get socket of recipient
                    Socket recipientSocket = ClientSockets[arg_recipient];
                    // Gets all of [message]
                    string arg_message = String.Join(' ', message, 2, message.Length - 2);

                    // When [message] is whitespace
                    if (String.IsNullOrWhiteSpace(arg_message))
                    {
                        response = "Invalid SEND: [message] IsNullOrWhiteSpace";
                    }
                    // When [message] is not whitespace
                    else
                    {
                        // Creates string to be sent to recipient
                        string recipientResponse = "MESSAGE " + recipientSocket.RemoteEndPoint + " " + arg_message;
                        // Sends message
                        SendString(recipientResponse, recipientSocket);
                        // Outputs debug message to console: what the recipient is being sent
                        PrintMessage($"To   {recipientSocket.RemoteEndPoint} - {recipientResponse}");
                        // Sender client will receive confirmation message
                        response = "SUCCESSFUL SEND";
                    }
                }
                // If [recipient] is invalid
                else
                {
                    response = $"Invalid SEND: Invalid [recipient] \"{arg_recipient}\"";
                }
            }

            return response;
        }

        private string Command_LOGIN(string[] message)
        {
            string response = "InvalidLogin: Unknown error";

            // if not: "Login: [username] [password]"
            if (message.Length != 3)
            {
                response = "InvalidLogin: Invalid number of credentials supplied";
            }
            else
            {
                string username = message[1];
                string password = message[2];
                bool logged_in = false;

                IEnumerable<Account> accounts = CSVHandler.GetAccounts();

                //Compares the username and password against all of those in the CSV
                foreach (Account account in accounts)
                {
                    PrintMessage($"{account.Username},{account.Password},{account.PublicKey},{account.PrivateKey}");

                    // When there is a match, set login flag and break loop
                    if (account.Username == username && account.Password == password)
                    {
                        response = $"ValidLogin: Welcome {username}!";
                        logged_in = true;
                    }
                }

                // When the supplied credentials don't match any of those in the CSV
                if (!logged_in)
                {
                    response = "InvalidLogin: User matching your credentials doesn't exist";
                }
            }

            return response;
        }

        // Neatly output messages to the server console
        public static void PrintMessage(string message)
        {
            string time = DateTime.Now.ToString("HH:mm ss.fff");
            Console.WriteLine($"[{time}] {message}");

            /* TODO: Fix pretty console output
                        
            //Error may be thrown by 'hitting' top of screen
            try
            {
                // Ensures blank line above is actually clear
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));

                // Clears closing instruction message
                Console.SetCursorPosition(0, Console.CursorTop + 1);
                Console.Write(new string(' ', Console.WindowWidth));

                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
            catch (Exception) { }

            // Prefix is the time padded into set length string
            string prefix = $"[{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}] ";

            int writeable = Console.WindowWidth - prefix.Length;
            if (message.Length > writeable)
            {
                // Prints lines that exceed window width
                for (int line = 1; message.Length > writeable; ++line)
                {
                    // For the first line
                    if (line == 1) { Console.WriteLine(prefix + message.Substring(0, writeable)); }
                    // All other lines need indentations
                    else {
                        Console.SetCursorPosition(prefix.Length, Console.CursorTop);
                        Console.WriteLine(message.Substring(0, writeable));
                    }

                    // Removes printed section
                    message = message.Remove(0, writeable);
                }

                // Prints remainder
                Console.SetCursorPosition(prefix.Length, Console.CursorTop);
                Console.WriteLine(message);
            }
            else
            {
                Console.WriteLine(prefix + message);
            }
                       

            Console.Write("\n[Enter to close server]");
            /**/
        }
    }
}
