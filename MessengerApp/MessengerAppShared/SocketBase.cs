using System.Net;
using System.Net.Sockets;

namespace MessengerAppShared
{
    public abstract class SocketBase
    {
        protected IPEndPoint EndPoint;
        protected Socket Socket;

        // Buffer to hold read in data, maximum of 2048 bytes
        public byte[] Buffer = new byte[4096];

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

        // Close and dispose of socket
        public void Disconnect()
        {
            // Disables sending and receiving from the socket
            Socket.Shutdown(SocketShutdown.Both);
            // Closes the socket and releases all its resources
            Socket.Close();
        }
    }
}
