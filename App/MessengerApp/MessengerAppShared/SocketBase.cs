﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MessengerAppShared
{
    public abstract class SocketBase
    {
        public IPEndPoint EndPoint;
        public Socket Socket;

        public byte[] Buffer = new byte[1024];

        // Constructor asigns endpoint and creates socket
        public SocketBase(int port = 31416)
        {
            EndPoint = new IPEndPoint(IPAddress.Loopback, port);
            Socket = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        // Starts receive from socket
        public void Receive(Socket socket)
        {
            // Starts listening for data from client
            socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }

        // Ends receive from socket (child forced to override)
        public abstract void ReceiveCallback(IAsyncResult asyncResult);

        // Starts send of string to socket
        public void Send(Socket socket, string text)
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
