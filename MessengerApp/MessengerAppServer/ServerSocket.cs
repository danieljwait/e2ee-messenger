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

        // Starts socket listening on a port, begins accept client loop
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

        // Disconnects socket to end server
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

            PrintMessage($"Client {clientSocket.RemoteEndPoint} connected");

            // Starts listening for data from the new client
            Receive(clientSocket);

            // Starts accepting the next client (continues infinite loop)
            Socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

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
            Send(response, senderSocket);
        }

        // Format: ECHO [message]
        private string Command_ECHO(string[] message)
        {
            string response;

            // If there is no [message] parameter, send error to client
            if (message.Length <= 1)
            {
                response = "Invalid ECHO: Missing argument [message]";
            }
            // If there is a [message] parameter, process message
            else
            {
                // Gets all of [message] to be echoed back
                string arg_message = String.Join(' ', message, 1, message.Length - 1);

                // If [message] is whitespace, send error to client
                if (String.IsNullOrWhiteSpace(arg_message))
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
            string response;

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
                        Send(recipientResponse, recipientSocket);
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
            else
            {
                response = "Invalid SEND: Unexpected error";
            }

            return response;
        }

        // Neatly output messages to the server console
        public static void PrintMessage(string message)
        {
            Console.WriteLine(message);

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
