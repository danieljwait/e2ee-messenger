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

        public byte[] Buffer = new byte[2048];

        // Constructor asigns endpoint and creates socket
        public SocketBase(int port = 31416)
        {
            EndPoint = new IPEndPoint(IPAddress.Loopback, port);
            Socket = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public static bool IsConnected(Socket socket)
        {
            try
            {
                // return true if client socket responds to poll and data is available to receive
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        // Starts receive from socket, virtual so clientSocket can extend it
        public virtual void Receive(Socket socket)
        {
            // Starts listening for data
            socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }

        //public void Receive(SocketBase socketObject)
        //{
        //    // Starts listening for data
        //    socketObject.Socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socketObject);
        //}

        // Ends receive from socket (child forced to override)
        public abstract void ReceiveCallback(IAsyncResult asyncResult);

        // Starts send of string to socket
        public void Send(string text, Socket socket)
        {
            // Converts string to bytes
            byte[] data = new Protocol(text).Data;
            // Sends the message, callback called when finished
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
        }

        // Ends send of string to socket
        public void SendCallback(IAsyncResult asyncResult)
        {
            // Gets socket from the async result
            Socket socket = (Socket)asyncResult.AsyncState;
            // Completes send
            socket.EndSend(asyncResult);
        }
    }
}
