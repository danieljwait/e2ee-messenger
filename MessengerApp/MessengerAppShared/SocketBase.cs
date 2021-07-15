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

        public static bool IsConnected(Socket socket)
        {
            try
            {
                // True if client socket responds to poll and data is available to receive
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }

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
            string text = new Protocol(dataBuffer).Text;

            // Handles message contents
            HandleMessage(socketHandler, text);
        }

        // Handles the message after its been received
        public abstract void HandleMessage(Socket socket, string message);

        // Starts send of string to socket
        public void Send(string text, Socket socket)
        {
            // Converts text to binary via predefined protocol
            byte[] data = new Protocol(text).Data;
            // Sends data
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
        }

        // Ends send of string to socket
        public void SendCallback(IAsyncResult asyncResult)
        {
            // Recreates socket
            Socket socket = (Socket)asyncResult.AsyncState;
            // Completes send
            socket.EndSend(asyncResult);
        }
    }
}
