using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MessengerAppShared
{
    public abstract class SocketBase
    {
        public IPEndPoint EndPoint;
        public Socket Socket;

        // Buffer to hold read in data, maximum of 2048 bytes
        public byte[] Buffer = new byte[2048];

        // Constructor creates socket
        public SocketBase(int port = 31416)
        {
            // Endpoint on local interface (not open Internet)
            EndPoint = new IPEndPoint(IPAddress.Loopback, port);
            // New TCP socket
            Socket = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        // Boolean of connection status
        public static bool IsConnected(Socket socket)
        {
            try
            {
                // True if client socket responds to poll and data is available to receive
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        // Starts async receive of a serialised object
        public void ReceiveObject(Socket socket)
        {
            // Starts reading data into Buffer array, starting at index 0 for a max of Buffer.Length bytes (2048)
            socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveObjectCallback), socket);
        }

        // Ends async receive of a serialised object
        public void ReceiveObjectCallback(IAsyncResult asyncResult)
        {
            // Recreates socket to handle connection
            Socket socketHandler = (Socket)asyncResult.AsyncState;

            // Reads data to buffer, gets the number of bytes received
            int received_binary = socketHandler.EndReceive(asyncResult);
            // Creates array to hold the data received
            byte[] dataBuffer = new byte[received_binary];
            // Copy received bytes to actual binary array
            Array.Copy(Buffer, dataBuffer, received_binary);

            // Deserialises object from binary
            object received_object = MessageBase.Deserialise(dataBuffer);
            // Handles the object
            HandleObject(socketHandler, received_object);

            // Continues infinite receive loop
            ReceiveObject(socketHandler);
        }

        // Will handle the received object
        public abstract void HandleObject(Socket socket, object received_object);

        // TODO: Remove, redundant
        // Starts async send of string
        public void SendString(string text, Socket socket)
        {
            // Converts text to binary via predefined protocol
            byte[] data = MessageBase.StringToBinary(text);
            // Sends data
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
        }

        // Starts async send of serialised object
        public void SendObject(object object_send, Socket socket)
        {
            // Serialises object into binary
            byte[] serialised = MessageBase.Serialise(object_send);
            // Sends data
            socket.BeginSend(serialised, 0, serialised.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
        }

        // Ends aync send
        public void SendCallback(IAsyncResult asyncResult)
        {
            // Recreates socket handler
            Socket socket = (Socket)asyncResult.AsyncState;
            // Completes send
            socket.EndSend(asyncResult);
        }






        // *** Old text based networking (still in ShellViewModel) ***

        // Starts to receive data from socket, virtual so can be extended
        public virtual void Receive(Socket socket)
        {
            // Starts reading data into Buffer array, starting at index 0 for a max of Buffer.Length bytes (2048)
            socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }

        // Ends receive from socket, child will extend if needed
        public virtual void ReceiveCallback(IAsyncResult asyncResult)
        {
            // Recreates socket to handle connection
            Socket socketHandler = (Socket)asyncResult.AsyncState;

            // Reads data to buffer, gets the number of bytes received
            int received = socketHandler.EndReceive(asyncResult);
            // Creates array to hold the data received
            byte[] dataBuffer = new byte[received];
            // Copy received bytes to actual binary array
            Array.Copy(Buffer, dataBuffer, received);

            // Converts received binary to text via predefined protocol
            string text = MessageBase.BinaryToString(dataBuffer);

            // Handles message contents
            HandleMessage(socketHandler, text);
        }

        // Evoked by Receive Callback
        // Handles the message after its been received
        public abstract void HandleMessage(Socket socket, string message);
    }
}
