using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class BMethods
    {
        static public byte[] ConvertHexToByte(string hex) //turns hex string to binary array
        {
            return Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray(); ;
        }
        static public string BytesToId(byte[] rv1)
        {
            return BitConverter.ToString(rv1).Replace("-", string.Empty);
        }
        static public byte[] Xor(byte[] left, byte[] right) //binary-wise xor of two byte arrays
        {
            byte[] val = new byte[left.Length];
            for (int i = 0; i < left.Length; i++)
                val[i] = (byte)(left[i] ^ right[i]);

            return val;
        }
        static public string BytesToString(byte[] bytes) //turns byte array to string
        {
            string byteString = "";
            foreach (byte i in bytes)
            {
                byteString += (Convert.ToString(i, 2).PadLeft(8, '0'));
            }
            return byteString;
        }
    }
}
