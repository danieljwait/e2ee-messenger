using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace MessengerAppShared
{
    public static class Protocol
    {
        public static byte[] Serialise(object data)
        {
            // Creates stream to handle data
            using (MemoryStream memory_stream = new MemoryStream())
            {
                // Binary formatter traverses object converting it all to binary
                BinaryFormatter binary_formatter = new BinaryFormatter();
                // Object is serialised into the stream of binary
                binary_formatter.Serialize(memory_stream, data);

                return memory_stream.ToArray();
            }
        }

        public static object Deserialise(byte[] data)
        {
            // Binary array converted into a Stream
            using (MemoryStream memory_stream = new MemoryStream(data))
            {
                // Creates formatter to handle Stream
                BinaryFormatter binary_formatter = new BinaryFormatter();
                // Stream is deserialised into an object
                object deserialised_object = binary_formatter.Deserialize(memory_stream);

                return deserialised_object;
            }
        }

        // Extracts and deserialises an object from an AsyncResult
        public static object GetObject(IAsyncResult asyncResult, byte[] buffer)
        {
            // Recreates socket to handle connection
            Socket socketHandler = (Socket)asyncResult.AsyncState;

            // Reads data to buffer, gets the number of bytes received
            int received_binary = socketHandler.EndReceive(asyncResult);
            // Creates array to hold the data received
            byte[] dataBuffer = new byte[received_binary];
            // Copy received bytes to actual binary array
            Array.Copy(buffer, dataBuffer, received_binary);

            // Deserialises object from binary
            return Deserialise(dataBuffer);
        }
    }
}
