using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Token
    {
        private byte[] value;
        private DateTime creationTime;
        public Token(byte[] value)
        {
            this.value = value;
            this.creationTime = DateTime.Now;
        }
        public bool UpToDate()
        {
            if (DateTime.Now.Subtract(this.creationTime).TotalMinutes < 8)
                return true;
            return false;
        }
        public byte[] GetValue()
        {
            return this.value;
        }
    }
}
