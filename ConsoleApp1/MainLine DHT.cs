using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace ConsoleApp1
{
    class MainLine_DHT
    {
        protected string id;
        protected byte[] IdBytes;
        protected string stringBytes;
        protected BTree<Bucket> Routing_Table;
        protected const char left = '1';
        protected const char right = '0';
        public MainLine_DHT()
        {
            Random rnd = new Random(); //generates custom id
            this.IdBytes = new byte[20];
            rnd.NextBytes(IdBytes);//makes random byte array composed of 20 bytes
            this.id = BitConverter.ToString(IdBytes).Replace("-", string.Empty); //changes bytes to hex string simailair to sha 1
            this.id = this.id.ToLower();
            this.stringBytes = BMethods.BytesToString(IdBytes);
            this.Routing_Table = new BTree<Bucket>(new Bucket());
        }
        public void AddNode(string hex)
        {
            BTree<Bucket> pos = this.Routing_Table;
            byte[] NodeId = BMethods.ConvertHexToByte(hex);
            byte[] Distance = BMethods.Xor(NodeId, this.IdBytes);
            string DistanceStr = BMethods.BytesToString(Distance);
            int index = 0;
            while(DistanceStr[index]!='1')
            {
                if(stringBytes[index]==left)
                {
                    if(!pos.HasLeft())
                    {
                        if(!pos.GetValue().AddNode(hex, IdBytes))//if its full and cant add
                        {
                            if (index == 160)
                                return;
                            if(stringBytes[index+1] == left)
                            {
                                pos.SetLeft(new BTree<Bucket>(pos.GetValue().split(index)));
                                pos.GetLeft().GetValue().AddNode(hex, this.IdBytes);
                            }
                            else if (stringBytes[index + 1] == right)
                            {
                                pos.SetRight(new BTree<Bucket>(pos.GetValue().split(index)));
                                pos.GetRight().GetValue().AddNode(hex, this.IdBytes);

                            }
                        }
                        return;
                    }
                    pos = pos.GetLeft();
                }
                else if (stringBytes[index] == right)
                {
                    if (!pos.HasRight())
                    {
                        if (!pos.GetValue().AddNode(hex, IdBytes))//if its full and cant add
                        {
                            if (index == 160)
                                return;
                            if (stringBytes[index + 1] == left)
                            {
                                pos.SetLeft(new BTree<Bucket>(pos.GetValue().split(index)));
                                pos.GetValue().AddNode(hex, this.IdBytes);
                            }
                            else if (stringBytes[index + 1] == right)
                            {
                                pos.SetRight(new BTree<Bucket>(pos.GetValue().split(index)));
                                pos.GetValue().AddNode(hex, this.IdBytes);

                            }
                        }
                        return;
                    }
                    pos = pos.GetRight();
                }
                index++;

            }
            if (!pos.GetValue().AddNode(hex, this.IdBytes))//if its full and cant add
            {
                if (index == 160)
                    return;
                if (stringBytes[index + 1] == right)
                {
                    pos.SetRight(new BTree<Bucket>(pos.GetValue().split(index)));
                    pos.GetRight().GetValue().AddNode(hex, this.IdBytes);
                }
                else if (stringBytes[index + 1] == left)
                {
                    pos.SetLeft(new BTree<Bucket>(pos.GetValue().split(index)));
                    pos.GetLeft().GetValue().AddNode(hex, this.IdBytes);
                }
            }

        }
        public void PrintTree(BTree<Bucket> tree, int i, Node<Queue<Bucket>> layers)
        {
            if (tree != null)
            {
                int j = i;
                Node<Queue<Bucket>> layers_start = layers;
                for (; j > 0 && layers.GetNext() != null; j--)
                {
                    layers = layers.GetNext();
                }
                if (layers.GetNext() == null && j != 0)
                {
                    layers.SetNext(new Node<Queue<Bucket>>(new Queue<Bucket>()));
                    layers = layers.GetNext();
                }
                layers.GetValue().Insert(tree.GetValue());
                PrintTree(tree.GetRight(), i + 1, layers_start);
                PrintTree(tree.GetLeft(), i + 1, layers_start);
            }
            else
                return;

        }
        public void PrintTree()
        {
            BTree<Bucket> tree = this.Routing_Table;
            Console.WriteLine(tree.GetValue());
            Console.WriteLine("------");
            Node<Queue<Bucket>> layers = new Node<Queue<Bucket>>(new Queue<Bucket>());
            PrintTree(tree.GetLeft(), 0, layers);
            PrintTree(tree.GetRight(), 0, layers);
            int i = 1;
            Console.WriteLine(tree.GetValue());
            while (layers != null)
            {
                Console.WriteLine();
                while (!layers.GetValue().IsEmpty())
                {
                    Console.Write(layers.GetValue().Remove() + " ");
                }
                layers = layers.GetNext();
                Console.WriteLine("------------");
            }
        }
    }
}
