using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class DHTNode
    {
        private string nodeId;
        private byte[] xorVal;
        private string bstring;
        private string xorString;

        public string NodeId { get => nodeId; set => nodeId = value; }
        public byte[] XorVal { get => xorVal; set => xorVal = value; }
        public string Bstring { get => bstring; set => bstring = value; }
        public string XorString { get => xorString; set => xorString = value; }

        public DHTNode(string NodeId, byte[] XorVal,string Bstring, string XorString)
        {
            this.NodeId = NodeId;
            this.XorVal = XorVal;
            this.Bstring = Bstring;
            this.XorString = XorString;
        }
    }
}
