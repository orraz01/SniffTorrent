using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BencodeNET;
using System.Collections;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Random rnd = new Random();
            byte[] id = new byte[20];
            rnd.NextBytes(id);


            string hex = BitConverter.ToString(id);
            hex = BitConverter.ToString(id).Replace("-", string.Empty);
            hex = hex.ToLower();
            byte[] raz= Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray(); ;
            Console.WriteLine(hex);
            Console.WriteLine();
            string count = "";
            byte[] rv1 = new byte[20];
            byte[] rv2 = new byte[20];
            rnd.NextBytes(rv1);
            rnd.NextBytes(rv2);
            var result = ((IStructuralComparable)rv1).CompareTo(rv2, Comparer<byte>.Default);
            string myId = BitConverter.ToString(rv1).Replace("-", string.Empty);
            Console.WriteLine(myId);
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder(myId);
            strBuilder[0] = '5';
            string str = strBuilder.ToString();
            strBuilder = new System.Text.StringBuilder(myId);
            strBuilder[1] = '5';
            str = strBuilder.ToString();
            MainLine_DHT test = new MainLine_DHT();
            for(int i=0;i<1000;i++)
            {
                rnd.NextBytes(rv2);
                str = BitConverter.ToString(rv2).Replace("-", string.Empty);
                test.AddNode(str);

            }
            Console.WriteLine("myid:"+BMethods.BytesToString(BMethods.ConvertHexToByte(myId)));

            test.PrintTree();

            /*BitArray myBA = new BitArray(raz);
            count = 0;
            foreach (bool i in myBA)
            {
                Console.WriteLine(i);
                count++;
            }
            Console.WriteLine($"\n {count}");*/
        }
    }
}
