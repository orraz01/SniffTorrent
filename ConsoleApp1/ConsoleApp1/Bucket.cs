using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Net;
using System.Net.Sockets;

namespace ConsoleApp1
{
    class Bucket
    {
        private Node<DHTNode> bucket;
        private int index;
        public int Index { get => index; set => index = value; }
        public Bucket()
        {
            this.bucket = null;
            this.index = 0;
        }
        private Bucket(Node<DHTNode> pos,int index)
        { 
            this.bucket = pos;
            this.index = index;
        }
        private int NodeRange(byte [] xor)
        {
            int count = 0;
            while (xor[count] == 0)
            {
                count++;
            }
            return count;
        }
        public bool IsFull()
        {
            return this.index ==8;
        }
        public bool AddNode(string id, byte[] myId,IPAddress address, int port) //add node to bucket, delete and redo
        {
            Node<DHTNode> pos = bucket;
            if (this.index == 8)
                return false; //bucket is full
            if (BMethods.ConvertHexToByte(id).SequenceEqual(myId))
                return true;
            else
            {
                if (bucket == null)
                {
                    if (id == BMethods.BytesToId(myId))
                        return true;
                    this.bucket = new Node<DHTNode>(new DHTNode(id, myId, address, port));
                    this.index++;
                    return true;
                }
                byte[] XorValue = BMethods.Xor(myId, BMethods.ConvertHexToByte(id));
                while (pos.GetNext() != null)
                {
                    var result = ((IStructuralComparable)XorValue).CompareTo(pos.GetValue().XorVal, Comparer<byte>.Default);
                    if (result < 0)
                    {
                        pos = pos.GetNext();
                    }
                    else if (result == 0)
                    {
                        //udapte node
                        pos.GetValue().Update();
                        return true;
                    }
                    else
                    {
                        Node<DHTNode> pos2 = new Node<DHTNode>(pos.GetValue(), pos.GetNext());
                        pos.SetValue(new DHTNode(id, myId, address, port));
                        pos.SetNext(pos2);
                        this.index++;
                        return true;
                    }
                }
                var result2 = ((IStructuralComparable)XorValue).CompareTo(pos.GetValue().XorVal, Comparer<byte>.Default);

                if (result2 > 0)
                {
                    Node<DHTNode> pos2 = new Node<DHTNode>(pos.GetValue(), pos.GetNext());
                    pos.SetValue(new DHTNode(id, myId, address, port));
                    pos.SetNext(pos2);
                    this.index++;
                    return true;
                }
                if (result2 == 0)
                    return true;
                pos.SetNext(new Node<DHTNode>(new DHTNode(id, myId, address, port)));
                this.index++;
                return true;
            }
        }
        public DHTNode LeastSeenNode()
        {
            Node<DHTNode> pos = this.bucket;
            DHTNode leastSeen = pos.GetValue();
            pos = pos.GetNext();
            while(pos!=null)
            {
                if(pos.GetValue().LastSeen<leastSeen.LastSeen)
                {
                    leastSeen = pos.GetValue();
                }
                pos = pos.GetNext();
            }
            return leastSeen;
        }
        public void UpdateNode(DHTNode node) 
        {
            Node<DHTNode> pos = this.bucket;
            while(pos!=null && pos.GetValue()!=node)
            {
                pos = pos.GetNext();
            }
            if(pos!=null)
                pos.GetValue().Update();
        }
        public void RemoveNode(DHTNode node)
        {
            if (this.bucket == null)
                return;
            if (this.bucket.Remove(node))
                this.index--;
        }
        public Bucket split(int range) //update split again
        {
            int count = 1;
            Node<DHTNode> pos = bucket;
            if(pos.GetValue().XorString[range]=='0')
            {
                this.bucket = null;
                int save_index = this.index;
                this.index = 0;
                return new Bucket(pos,save_index);
            }
            while(pos.GetNext()!=null)
            {
                if(pos.GetNext().GetValue().XorString[range]== '0')
                {
                    Node<DHTNode> pos2 = pos.GetNext();
                    pos.SetNext(null);
                    int save_index = this.index;
                    this.index = count;
                    return new Bucket(pos2, save_index - count);
                }
                count++;
                pos = pos.GetNext();
            }
            return new Bucket();
        }
        public DHTNode[] Return_a_Closest(int num)
        {
            if (num > Index)
            {
                DHTNode[] arr1 = new DHTNode[index];
                Node<DHTNode> pos1 = bucket;
                for (int i = 0; i < arr1.Length; i++)
                {
                    arr1[i] = pos1.GetValue();
                    pos1 = pos1.GetNext();
                }
                return arr1;
            }
            DHTNode[] arr2 = new DHTNode[num];
            Node<DHTNode> pos2 = bucket;
            for (int i = 0; i < (index - num); i++)
            {
                pos2 = pos2.GetNext();
            }
            for (int i = 0; i < arr2.Length; i++)
            {
                arr2[i] = pos2.GetValue();
                pos2 = pos2.GetNext();
            }
            return arr2;
        }
        public byte[] NodesList()
        {
            byte[] returnArr = new byte[index * 26];
            Node<DHTNode> pos = this.bucket;
            for(int i=0; i<returnArr.Length;i+=26)
            {
                byte[] id = BMethods.ConvertHexToByte(pos.GetValue().NodeId);
                Array.Copy(id, 0, returnArr, i, 20);
                byte[] ip = pos.GetValue().Address.GetAddressBytes();
                Array.Copy(ip, 0, returnArr, i+20, 4);
                byte[] port = BitConverter.GetBytes(Convert.ToUInt16(pos.GetValue().Port));
                byte placeholder = port[1];
                port[1] = port[0];
                port[0] = placeholder;
                Array.Copy(port, 0, returnArr, i + 24, 2);
                pos = pos.GetNext();
            }
            return returnArr;
        }
        public override string ToString()
        {
            Node<DHTNode> pos = bucket;
            string text = "";
            while (pos != null)
            {
                text += pos.GetValue().NodeId+"\n ";
                pos = pos.GetNext();
            }
            return text;
        }
        
    }
}