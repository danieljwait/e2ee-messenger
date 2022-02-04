using MessengerAppShared;
using MessengerAppShared.Messages;
using MessengerAppShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace MessengerAppServer
{
    public class ServerSocket : SocketBase
    {
        // For not logged in users
        private List<Socket> NotAuthedUsers = new List<Socket>();

        // For logged in users <username, socket>
        private Dictionary<string, Socket> AuthedUsers = new Dictionary<string, Socket>();
        private Dictionary<Socket, string> AuthedUsersReverse = new Dictionary<Socket, string>();

        // Maximum length of the connection queue for new clients
        private const int BACKLOG = 5;

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
            try { Disconnect(); }
            // TODO: Find cause of exception causing clients to disconnect
            catch (Exception e) { PrintMessage("Error: " + e.Message); }

            PrintMessage("Server stopped");
        }

        // Accepts incoming client connection
        private void AcceptCallback(IAsyncResult asyncResult)
        {
            // Creates a socket for the new client to handle the connection
            Socket clientSocket = Socket.EndAccept(asyncResult);

            // Add user to list of not signed in users
            AddUser(clientSocket);

            // Starts listening for data from the new client
            ReceiveObject(clientSocket);

            // Starts accepting the next client (continues infinite loop)
            Socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        // Starts receive of data from client
        private void ReceiveObject(Socket socket)
        {
            // Starts reading data into Buffer array, starting at index 0 for a max of Buffer.Length bytes (2048)
            socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveObjectCallback), socket);
        }

        // Finishes receive of data from client, then starts loop again
        private void ReceiveObjectCallback(IAsyncResult asyncResult)
        {
            //try
            //{
                // Recreates socket to handle connection
                Socket socketHandler = (Socket)asyncResult.AsyncState;
                // Deserialises object from binary
                object received_object = Protocol.GetObject(asyncResult, Buffer);
                // Handles the object, gets exit code
                int exit_code = Handle(socketHandler, (ClientToServerMessage)received_object);

                // An exit code of 1 means the client is disconnecting
                if (exit_code == 1)
                {
                    // Remove client from dictionaries and close socket
                    RemoveUser(socketHandler);
                    return;
                }

                // Continues infinite receive loop
                ReceiveObject(socketHandler);
            //}
            //catch (Exception e)
            //{
            //    PrintMessage($"Error: {e.Message}");
            //    PrintMessage($"Action: Ending conversation with culprit client");
            //}
        }

        // Moves user from not authorised status to authorised
        private void MakeUserAuthed(string username, Socket socket)
        {
            // Move to dictionaries of authorised users
            NotAuthedUsers.Remove(socket);
            AuthedUsers.Add(username, socket);
            AuthedUsersReverse.Add(socket, username);

            PrintMessage($"{socket.RemoteEndPoint} logged in as '{username}'");
        }

        // Sends all connected users the new user
        private void TellClientUserConnect(string username)
        {
            // For each connected and authorised client
            foreach (Socket connectedSocket in AuthedUsers.Values)
            {
                // Send new username
                var ClientMessage = new ServerToClientMessage(ServerCommand.ClientConnect, username);
                SendToClient(ClientMessage, connectedSocket);
            }

            PrintMessage($"Updated clients' about new user");
        }

        // Sends all connected users the disconnected user
        private void TellClientUserDisconnect(string username)
        {
            // For each connected and authorised client
            foreach (Socket connectedSocket in AuthedUsers.Values)
            {
                // Send disconnected username
                var ClientMessage = new ServerToClientMessage(ServerCommand.ClientDisconnect, username);
                SendToClient(ClientMessage, connectedSocket);
            }

            PrintMessage($"Updated clients' about disconnected user");
        }

        private void SendClientList(Socket client_socket)
        {
            List<string> clients = AuthedUsers.Keys.ToList();
            var ClientMessage = new ServerToClientMessage(ServerCommand.SendClientsList, clients);
            SendToClient(ClientMessage, client_socket);

            PrintMessage($"Sent {AuthedUsersReverse[client_socket]} list of clients");
        }

        // Adds a user who is not logged in
        private void AddUser(Socket socket)
        {
            // Add user to list of not signed in users
            NotAuthedUsers.Add(socket);

            PrintMessage($"{socket.RemoteEndPoint} connected");
        }

        // Move user from authed to not authed
        private void MakeUserNotAuthed(Socket socket)
        {
            // Gets username
            string username = AuthedUsersReverse[socket];
            // Removes from dictionaries
            AuthedUsers.Remove(username);
            AuthedUsersReverse.Remove(socket);

            // Move to dictionaries of authorised users
            NotAuthedUsers.Add(socket);

            PrintMessage($"{socket.RemoteEndPoint} logged out of '{username}'");
        }

        // Removes a client
        private void RemoveUser(Socket socket)
        {
            // Remove client from list (not authed) or dictionaries (authed)
            if (!NotAuthedUsers.Remove(socket))
            {
                // Gets username
                string username = AuthedUsersReverse[socket];
                // Removes from dictionaries
                AuthedUsers.Remove(username);
                AuthedUsersReverse.Remove(socket);
            }

            // Closes connection with socket
            socket.Disconnect(false);

            PrintMessage($"{socket.RemoteEndPoint} disconnected");
        }

        // Delegates received message to relevant routine
        private int Handle(Socket socket, ClientToServerMessage message)
        {
            switch (message.Command)
            {
                case ClientCommand.Disconnect:
                    RemoveUser(socket);
                    return 1;

                case ClientCommand.LogOut:
                    TellClientUserDisconnect(AuthedUsersReverse[socket]);
                    MakeUserNotAuthed(socket);
                    break;

                case ClientCommand.Login:
                    VerifyLogin(socket, (Dictionary<string, string>)message.Data);
                    break;

                case ClientCommand.Message:
                    ProcessMessage((MessageModel)message.Data);
                    break;
            }
            return 0;
        }

        // Routes the message to intended recipient
        private void ProcessMessage(MessageModel data)
        {
            PrintMessage($"Received message from {data.Sender} to {data.Recipient}");

            // Get the socket from the dictionary using the username in the Recipient field
            var recipient_socket = AuthedUsers[data.Recipient];

            // Message for client telling them of the new message
            var client_message = new ServerToClientMessage(ServerCommand.Message, data);

            // Routes message to recipient
            SendToClient(client_message, recipient_socket);

            PrintMessage($"Sent message to {data.Recipient}");
        }

        // Checks if username and password are in CSV of credentials
        private bool CheckCredentials(string username, string password)
        {
            // Gets all the credentials from the CSV
            IEnumerable<Account> accounts = CSVHandler.GetAccounts();

            // Compares the username and password against all of those in the CSV
            foreach (Account account in accounts)
            {
                // If there is a match, set login flag and break loop
                if (account.Username == username && account.Password == password)
                {
                    return true;
                }
            }

            // If not found
            return false;
        }

        // Processes a login request
        private void VerifyLogin(Socket socket, Dictionary<string, string> credentials)
        {
            // Prepare object to be returned to user, missing success/failure flag
            var response = new ServerToClientMessage(ServerCommand.LoginResult);

            // Checks CSV for credentials pair
            if (CheckCredentials(credentials["Username"], credentials["Password"]))
            {
                // Gets user's details to send back to client
                var user_credentials = new Dictionary<string, string>()
                {
                    { "Username", credentials["Username"] }
                };

                // Returning not false means valid
                response.Data = user_credentials;

                // Sends the result to client
                SendToClient(response, socket);

                // Move to dictionaries of authorised users
                MakeUserAuthed(credentials["Username"], socket);

                // Send client list of connected clients
                SendClientList(socket);

                // Tells other clients about the new login
                TellClientUserConnect(credentials["Username"]);
            }
            else
            {
                // Tells user login failed
                response.Data = false;

                // Sends the result to client
                SendToClient(response, socket);

                PrintMessage($"{socket.RemoteEndPoint} attempted login to '{credentials["Username"]}'");
            }
        }

        public void SendToClient(object message, Socket client_socket)
        {
            // Serialises object into binary
            byte[] serialised = Protocol.Serialise(message);
            // Sends data {buffer, offset, size, flags}
            client_socket.Send(serialised, 0, serialised.Length, SocketFlags.None);
        }

        // Output messages to the server console with time-stamp
        private static void PrintMessage(string message)
        {
            string time = DateTime.Now.ToString("HH:mm ss.fff");
            Console.WriteLine($"[{time}] {message}");
        }
    }
}
