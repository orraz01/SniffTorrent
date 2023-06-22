using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;


namespace ConsoleApp1
{
    class SendInfo
    {
        byte[] data;
        IPEndPoint point;
        public SendInfo(byte[] data, IPEndPoint point)
        {
            this.Data = data;
            this.Point = point;
        }

        public IPEndPoint Point { get => point; set => point = value; }
        public byte[] Data { get => data; set => data = value; }
    }
}
