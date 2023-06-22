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
    class PeerList
    {
        private List<IPEndPoint> peers;
        private Queue<DateTime> peersTime;
        public PeerList()
        {
            this.peers = new List<IPEndPoint>();
            this.peersTime = new Queue<DateTime>();
        }
        public void AddPeer(IPEndPoint peer)
        {
            if (!this.peers.Contains(peer))
            {
                this.peers.Add(peer);
                this.peersTime.Insert(DateTime.Now);
            }
        }
        public BList GetList()
        {
            BList returnlist = new BList();
            foreach(IPEndPoint point in this.peers)
            {
                byte[] returnArr = new byte[6];
                byte[] ip = point.Address.GetAddressBytes();
                Array.Copy(ip, 0, returnArr, 0, 4);
                byte[] port = BitConverter.GetBytes(Convert.ToUInt16(point.Port));
                Console.WriteLine(BitConverter.ToUInt16(port, 0));
                byte placeholder = port[1];
                port[1] = port[0];
                port[0] = placeholder;
                Array.Copy(port, 0, returnArr, 4, 2);
                returnlist.Add(new BString(returnArr));
            }
            return returnlist;
        }
        public void Update()
        {
            if(!this.peersTime.IsEmpty())
            {
                if (DateTime.Now.Subtract(this.peersTime.Head()).TotalMinutes < 45)
                    return;
                this.peersTime.Remove();
                this.peers.RemoveAt(0);
            }
        }
    }
}
