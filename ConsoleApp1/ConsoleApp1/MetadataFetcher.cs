using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using BencodeNET.Parsing;
using BencodeNET.Objects;

namespace ConsoleApp1
{
    class MetadataFetcher
    {
        private int port;
        private byte[] infohash;
        private IPEndPoint peer;
        private Socket connection;
        public MetadataFetcher(int port, byte[] infohash,IPEndPoint peer)
        {
            this.connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.port = port;
            this.infohash = infohash;
            this.peer = peer;
        }
        private byte[] Handshake(byte[] peerid, byte[] infohash)
        {
            byte[] ProtocolIdentifier = new byte[] { 0x13, 0x42, 0x69, 0x74, 0x54, 0x6f, 0x72, 0x72, 0x65, 0x6e, 0x74, 0x20, 0x70, 0x72, 0x6f, 0x74, 0x6f, 0x63, 0x6f, 0x6c };
            byte[] ReservedBytes = new byte[8];
            ReservedBytes[5] = 0x10;
            byte[] protocolHandshake = new byte[ProtocolIdentifier.Length + ReservedBytes.Length + peerid.Length + infohash.Length];
            int i = 0;
            Array.Copy(ProtocolIdentifier, 0, protocolHandshake, i, ProtocolIdentifier.Length);
            i += ProtocolIdentifier.Length;
            Array.Copy(ReservedBytes, 0, protocolHandshake, i, ReservedBytes.Length);
            i += ReservedBytes.Length;
            Array.Copy(infohash, 0, protocolHandshake, i, infohash.Length);
            i += ReservedBytes.Length; 
            Array.Copy(peerid, 0, protocolHandshake, i, ReservedBytes.Length);
            Array.Copy(infohash, 0, protocolHandshake, i, infohash.Length);
            return protocolHandshake;
        }
        private bool CheckValidHandshake(byte[] handshake)
        {
            byte[] ProtocolIdentifier = new byte[] { 0x13, 0x42, 0x69, 0x74, 0x54, 0x6f, 0x72, 0x72, 0x65, 0x6e, 0x74, 0x20, 0x70, 0x72, 0x6f, 0x74, 0x6f, 0x63, 0x6f, 0x6c };
            byte[] checkIdentifier = new byte[ProtocolIdentifier.Length];
            Array.Copy(handshake, 0, checkIdentifier, 0, checkIdentifier.Length);
            ByteArrayEqualityComparer comparer = new ByteArrayEqualityComparer();
            if (!comparer.Equals(ProtocolIdentifier, checkIdentifier))
            {
                Console.WriteLine("identifier doesnt match");
                return false;
            }
            byte[] reservedBytes = new byte[8];
            Array.Copy(handshake,ProtocolIdentifier.Length,reservedBytes,0,8);
            return (reservedBytes[5] & 0b00010000) != 0;
            //possibly add later infohash check
        }

        private byte [] Receive()
        {
            byte[] lengthPrefix = new byte[4];
            connection.Receive(lengthPrefix);
            int length = (lengthPrefix[0] << 24) | (lengthPrefix[1] << 16) | (lengthPrefix[2] << 8) | lengthPrefix[3];
            byte[] data = new byte[length];
            connection.Receive(data);
            return data;
        }
        private byte[] ExtendedHandshake(int id)
        {
            BDictionary handshake = new BDictionary();
            BDictionary mDict = new BDictionary();
            mDict["ut_metadata"] = new BNumber(id);
            handshake["m"] = mDict;
            byte[] prefix = new byte[] { 0x14, 0x00 };
            byte[] payload = handshake.EncodeAsBytes();
            int length = prefix.Length + payload.Length;
            byte[] LengthPrefix = new byte[4]; // Create a byte array with a length of 4

            LengthPrefix[0] = (byte)(length >> 24); // Extract the most significant byte (MSB) and store it in the first element
            LengthPrefix[1] = (byte)(length >> 16); // Extract the second most significant byte and store it in the second element
            LengthPrefix[2] = (byte)(length >> 8);  // Extract the third most significant byte and store it in the third element
            LengthPrefix[3] = (byte)length;

            byte[] handshakeArr = new byte[length+4];
            Array.Copy(LengthPrefix, 0, handshakeArr, 0, 4);
            Array.Copy(prefix, 0, handshakeArr, 4, 2);
            Array.Copy(payload, 0, handshakeArr,6, payload.Length);

            return handshakeArr;

        }
        private byte[] RequestMessage(int piece)
        {
            BDictionary requestMsg = new BDictionary();
            requestMsg["msg_type"] = new BNumber(0);
            requestMsg["piece"] = new BNumber(piece);
            piece++;
            byte[] prefix = new byte[] { 0x14, 0x02 };
            byte[] payload = requestMsg.EncodeAsBytes();
            int mesgLength = prefix.Length + payload.Length;
            byte[] LengthPrefix = new byte[4]; // Create a byte array with a length of 4

            LengthPrefix[0] = (byte)(mesgLength >> 24); // Extract the most significant byte (MSB) and store it in the first element
            LengthPrefix[1] = (byte)(mesgLength >> 16); // Extract the second most significant byte and store it in the second element
            LengthPrefix[2] = (byte)(mesgLength >> 8);  // Extract the third most significant byte and store it in the third element
            LengthPrefix[3] = (byte)mesgLength;

            byte[] msg = new byte[mesgLength + 4];
            Array.Copy(LengthPrefix, 0, msg, 0, 4);
            Array.Copy(prefix, 0, msg, 4, 2);
            Array.Copy(payload, 0, msg, 6, payload.Length);
            return msg;
        }
        public void FetchMetadata()
        {
            try
            {
                var parser = new BencodeParser(Encoding.GetEncoding("ISO-8859-1"));
                byte[] myId = new byte[20];
                Random rnd = new Random();
                rnd.NextBytes(myId);
                IPAddress ipAddress = IPAddress.Any; // IP address to bind to
                IPEndPoint localEndpoint = new IPEndPoint(ipAddress, this.port);
                connection.Bind(localEndpoint);
                connection.Connect(this.peer);
                byte[] customHandshake = this.Handshake(myId, this.infohash);
                connection.Send(customHandshake);
                byte[] handshakeReceived = new byte[68];
                connection.Receive(handshakeReceived);
                int length;
                if (CheckValidHandshake(handshakeReceived))
                {
                    byte[] data;
                    if (connection.Available == 0)
                    {
                        connection.Send(ExtendedHandshake(2));
                        data = Receive();
                        BDictionary payload = parser.Parse<BDictionary>(data);
                        BDictionary m = ((BDictionary)payload["m"]);
                        if (!m.ContainsKey(new BString("ut_metadata")))
                        {
                            Console.WriteLine("doesnt support ut_metadata");
                            return;
                        }
                        length = (BNumber)payload["metadata_size"];
                    }
                    else
                    {
                        data = Receive();
                        BDictionary payload = parser.Parse<BDictionary>(data);
                        BDictionary m = ((BDictionary)payload["m"]);
                        if (!m.ContainsKey(new BString("ut_metadata")))
                        {
                            Console.WriteLine("doesnt support ut_metadata");
                            return;
                        }
                        length = (BNumber)payload["metadata_size"];
                        int id = (BNumber)m["ut_metadata"];
                        connection.Send(ExtendedHandshake(id));

                    }
                    int index = 0;
                    int piece = 0;
                    byte[] metadata = new byte[length];
                    while (index != length)
                    {
                        byte[] requestmsg = RequestMessage(piece);
                        connection.Send(requestmsg);
                        byte[] dataReceived = Receive();
                        BDictionary testDict = parser.Parse<BDictionary>(dataReceived);

                        int queryLength = testDict.EncodeAsBytes().Length;
                        int metadataLength = (BNumber)testDict["total_size"];
                        byte[] arr = dataReceived;
                        // Copy the portion of the original array that we want to extract into the new array
                        Array.Copy(arr, length, metadata, index, queryLength);
                        index += metadataLength;
                        piece++;
                    }
                    BDictionary dic = parser.Parse<BDictionary>(metadata);
                    foreach (KeyValuePair<BString, IBObject> kvp in dic)
                    {
                        // Get the key and value for the current key-value pair
                        BString key = kvp.Key;
                        IBObject value = kvp.Value;

                        // Print the key and value to the console
                        Console.WriteLine("{0}: {1}", key.ToString(), value.ToString());
                    }
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                connection.Close();
            }


        }
    }
}
