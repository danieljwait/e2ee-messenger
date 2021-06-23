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
        // Dict of identifier and socket
        public Dictionary<string, Socket> ClientSockets = new Dictionary<string, Socket>();
        public Dictionary<Socket, string> ClientSocketsInverse = new Dictionary<Socket, string>();
        public const int BACKLOG = 5;

        // Starts listening
        public void Start()
        {
            PrintMessage("Starting server...");

            Socket.Bind(EndPoint);
            Socket.Listen(BACKLOG);

            PrintMessage("Server started");

            Socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        // Disconnects socket to end server
        public void Stop()
        {
            PrintMessage("Stopping server...");

            try
            {
                // Disables sending and receiving
                Socket.Shutdown(SocketShutdown.Both);
                // Ends connection
                Socket.Close();
            }
            // TODO: Find cause of exception causing clients to disconnect
            catch (Exception e) { PrintMessage("Error: " + e.Message); }

            PrintMessage("Server stopped");
        }

        // Accepts incoming client connection
        public void AcceptCallback(IAsyncResult asyncResult)
        {
            Socket clientSocket = Socket.EndAccept(asyncResult);
            ClientSockets.Add(clientSocket.RemoteEndPoint.ToString(), clientSocket);
            ClientSocketsInverse.Add(clientSocket, clientSocket.RemoteEndPoint.ToString());

            PrintMessage($"Client {clientSocket.RemoteEndPoint} connected");

            // Starts listening for data from the client just connected
            Receive(clientSocket);

            // Starts accepting the next client
            Socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        // Receives message and echoes it back to client
        public override void ReceiveCallback(IAsyncResult asyncResult)
        {
            // Gets socket from the async result
            Socket clientSocket = (Socket)asyncResult.AsyncState;

            // The amount of data received
            int received;
            // Only ends receive when client is still connected
            if (SocketBase.IsConnected(clientSocket))
            {
                received = clientSocket.EndReceive(asyncResult);
            }
            else
            {
                // Removes from connected clients dicts
                PrintMessage($"Client {ClientSocketsInverse[clientSocket]} has disconnected");
                ClientSockets.Remove(ClientSocketsInverse[clientSocket]);
                ClientSocketsInverse.Remove(clientSocket);
                return;
            }

            byte[] dataBuffer = new byte[received];
            // Copy received bytes to data buffer
            Array.Copy(Buffer, dataBuffer, received);
            // Converts received byte[] to string
            string text = new Protocol(dataBuffer).Text;

            // Calls function to handle message contents
            HandleMessage(clientSocket, text);

            // Loops back to start
            // TODO: Error after client disconnects; use try catch to check connection
            Receive(clientSocket);
        }

        public override void HandleMessage(Socket senderSocket, string message)
        {
            string cleaned = message.Replace("\n", "");
            string response;

            PrintMessage($"From {senderSocket.RemoteEndPoint} - {message}");

            if (String.IsNullOrWhiteSpace(message))
            {
                response = "Invalid message: IsNullOrWhiteSpace";
            }
            else
            {
                string[] parts = cleaned.Split(' ');
                string command = parts[0];

                if (command.ToUpper() == "ECHO")
                {
                    response = Command_ECHO(parts);
                }
                else if (command.ToUpper() == "SEND")
                {
                    response = Command_SEND(parts, senderSocket);
                }
                else
                {
                    response = $"Invalid command: Command \"{parts[0]}\" not found";
                }

            }

            PrintMessage($"To   {senderSocket.RemoteEndPoint} - {response}");
            Send(response, senderSocket);
        }

        private string Command_ECHO(string[] message)  // Format: ECHO [message]
        {
            string response;

            // If there is a [message]
            if (message.Length >= 2)
            {
                // Gets all of [message] to be ECHOed back
                string arg_message = String.Join(' ', message, 1, message.Length - 1);
                
                // If [message] is whitespace
                if (String.IsNullOrWhiteSpace(arg_message))
                {
                    response = "Invalid ECHO: [message] IsNullOrWhiteSpace";
                }
                // If [message] is not whitespace
                else
                {
                    response = arg_message;
                }
            }

            // If there is no [message]
            else
            {
                response = "Invalid ECHO: Missing argument [message]";
            }

            return response;
        }

        private string Command_SEND(string[] message, Socket senderSocket)  // Format: SEND [recipient] [message]
        {
            string response;

            // If there are no [recipient] and [message]
            if (message.Length == 1)
            {
                response = "Invalid SEND: Missing arguments [recipient] and [message]";
            }

            // If just [recipient]
            else if (message.Length == 2)
            {
                // Gets [recipient]
                string arg_recipient = message[1];

                // If [recipient] is in dictionary
                if (ClientSockets.ContainsKey(arg_recipient))
                {
                    response = "Invalid SEND: Missing argument [message]";
                }
                // If [recipient] is not in dictionary
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
                        // Does the SEND
                        string recipientResponse = "MESSAGE " + recipientSocket.RemoteEndPoint + " " + arg_message;
                        Send(recipientResponse, recipientSocket);
                        PrintMessage($"To   {recipientSocket.RemoteEndPoint} - {recipientResponse}");

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

                // Prints remainer
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
