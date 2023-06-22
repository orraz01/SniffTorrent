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
    class PingHandler
    {
        static Random rnd = new Random();
        private UdpClient connection;
        private DHTNode nodeToCheck;
        private DHTNode nodeToAdd;
        private Bucket bucket;
        private byte[] tkey;
        private byte[] data;
        private DateTime startingTime;
        private int TimeOuntCount;
        private byte[] mainId;
        static BencodeParser parser = new BencodeParser(Encoding.GetEncoding("ISO-8859-1"));

        public byte[] Data { get => data; set => data = value; }

        public PingHandler(UdpClient connection, DHTNode nodeToCheck, DHTNode nodeToAdd, Bucket bucket, byte[] mainId)
        {
            
            this.connection = connection;
            this.nodeToCheck = nodeToCheck;
            this.nodeToAdd = nodeToAdd;
            this.bucket = bucket;
            this.tkey = new byte[2];
            rnd.NextBytes(tkey);
            this.mainId = mainId;
            byte[] query = KRPC.PingReqest(mainId, tkey);
            this.connection.SendAsync(query, query.Length, new IPEndPoint(nodeToCheck.Address, nodeToCheck.Port));
            this.startingTime = DateTime.Now;
            this.TimeOuntCount = 0;

        }

        public bool Ping() //check ping
        {
            if (DateTime.Now.Subtract(this.startingTime).TotalSeconds > 10)
            {
                if (this.TimeOuntCount > 0)
                {
                    bucket.RemoveNode(nodeToCheck);
                    bucket.AddNode(nodeToAdd.NodeId, mainId, nodeToAdd.Address, nodeToAdd.Port);
                    return true;
                }
                this.TimeOuntCount++;
                this.startingTime = DateTime.Now;
                byte[] query = KRPC.PingReqest(mainId, tkey);
                connection.SendAsync(query, query.Length, new IPEndPoint(nodeToCheck.Address, nodeToCheck.Port));
                return false;
            }
            if (data == null)
            {
                return false;
            }
            BDictionary dictData = parser.Parse<BDictionary>(data);
            if(!dictData.ContainsKey("t"))
               return false;
            if (!((BString)dictData[new BString("t")]).Value.ToArray().SequenceEqual(this.tkey))
                return false;
            nodeToCheck.Update();
            return true;

        }
    }
}
