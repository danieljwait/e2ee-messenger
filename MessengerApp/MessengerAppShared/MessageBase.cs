using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MessengerAppShared
{
    // Base class for all objects sent through network
    [Serializable]
    public abstract class MessageBase
    {
        // TODO: Remove text-based networking from ShellViewModel
        // Helper: String -> Binary
        public static byte[] StringToBinary(string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        // Helper: Binary -> String
        public static string BinaryToString(byte[] input)
        {
            return Encoding.UTF8.GetString(input);
        }

        // Object -> Object's binary
        public static byte[] Serialise(object to_serialise)
        {
            // Creates stream to handle data
            using (MemoryStream memory_stream = new MemoryStream())
            {
                // Binary formatter traverses object converting it all to binary
                BinaryFormatter binary_formatter = new BinaryFormatter();
                // Object is serialised into the stream of binary
                binary_formatter.Serialize(memory_stream, to_serialise);

                return memory_stream.ToArray();
            }
        }

        // Object's binary -> Object
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
    }
}
