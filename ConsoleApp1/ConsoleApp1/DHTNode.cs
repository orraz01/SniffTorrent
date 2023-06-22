using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ConsoleApp1
{
    class DHTNode:IEquatable<DHTNode>
    {
        private string nodeId;
        private byte[] id;
        private byte[] xorVal;
        private string bstring;
        private string xorString;
        private IPAddress address;
        private int port;
        private DateTime lastSeen;

        public string NodeId { get => nodeId; set => nodeId = value; }
        public byte[] XorVal { get => xorVal; set => xorVal = value; }
        public string Bstring { get => bstring; set => bstring = value; }
        public string XorString { get => xorString; set => xorString = value; }
        public IPAddress Address { get => address; set => address = value; }
        public int Port { get => port; set => port = value; }
        public DateTime LastSeen { get => lastSeen; set => lastSeen = value; }
        public byte[] Id { get => id; set => id = value; }

        public DHTNode(string NodeId,byte[] MainId,IPAddress address,int port)
        {
            this.NodeId = NodeId;
            this.XorVal = BMethods.Xor(MainId, BMethods.ConvertHexToByte(nodeId));
            this.Bstring = BMethods.BytesToString(BMethods.ConvertHexToByte(NodeId));
            this.XorString = BMethods.BytesToString(this.XorVal);
            this.address = address;
            this.port = port;
            this.lastSeen = DateTime.Now;
            this.id = BMethods.ConvertHexToByte(this.NodeId);

        }
        public bool IsUpToDate()
        {
            if (DateTime.Now.Subtract(this.lastSeen).TotalMinutes < 15)
                return true;
            return false;
        }
        public void Update()
        {
            this.lastSeen = DateTime.Now;
        }
        public bool Equals(DHTNode obj)
        {
            if (!(obj is DHTNode))
                return false;
            DHTNode check = obj as DHTNode;
            return check == this;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is DHTNode))
                return false;
            DHTNode check = obj as DHTNode;
            return check == this;
        }
        public static bool operator==(DHTNode a, DHTNode b)
        {
            if (object.ReferenceEquals(a, b))
                return true;
            if (a.NodeId == b.NodeId && IPAddress.Equals(a.Address,b.Address) && a.Port == b.Port)
                return true;
            return false;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                const int seed = 17;
                const int multiplier = 23;
                int hash = seed;
                byte[] hash1 = BMethods.ConvertHexToByte(NodeId);
                foreach (byte b in hash1)
                {
                    hash = hash * multiplier + b.GetHashCode();
                }
                hash += address.GetHashCode();
                return hash;
            }
        }
        public static bool operator !=(DHTNode a, DHTNode b) => !(a == b);
    }
}
