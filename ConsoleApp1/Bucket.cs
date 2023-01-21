using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;


namespace ConsoleApp1
{
    class Bucket
    {
        protected Node<DHTNode> bucket;
        protected int index;


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
        public bool AddNode(string id, byte[] myId) //add node to bucket, delete and redo
        {
            Console.WriteLine(index);
            bool range0 = false;
            Node<DHTNode> pos = bucket;
            if (this.index == 7)
                return false; //bucket is full
            else
            {
                if (bucket == null)
                {
                    if (id == BMethods.BytesToId(myId))
                        return true;
                    bucket = new Node<DHTNode>(new DHTNode( id, BMethods.Xor(myId, BMethods.ConvertHexToByte(id)), BMethods.BytesToString(BMethods.ConvertHexToByte(id)), BMethods.BytesToString(BMethods.Xor(myId, BMethods.ConvertHexToByte(id))) ));
                    index++;
                    return true;
                }
                byte[] XorValue = BMethods.Xor(myId, BMethods.ConvertHexToByte(id));
                while (pos.GetNext() != null)
                {
                    var result = ((IStructuralComparable)XorValue).CompareTo(pos.GetValue().XorVal, Comparer<byte>.Default);
                    if (result<0)
                    {
                        pos = pos.GetNext();
                    }
                    else if(result==0)
                    {
                        return true;
                    }
                    else
                    {
                        Node<DHTNode> pos2 = new Node<DHTNode>(pos.GetValue(),pos.GetNext());
                        pos.SetValue(new DHTNode ( id, BMethods.Xor(myId, BMethods.ConvertHexToByte(id)), BMethods.BytesToString(BMethods.ConvertHexToByte(id)), BMethods.BytesToString(BMethods.Xor(myId, BMethods.ConvertHexToByte(id))) ));
                        pos.SetNext(pos2);
                        index++;
                        return true;
                    }
                }
                var result2 = ((IStructuralComparable)XorValue).CompareTo(pos.GetValue().XorVal, Comparer<byte>.Default);

                if (result2>0)
                {
                    Node<DHTNode> pos2 = new Node<DHTNode>(pos.GetValue(), pos.GetNext());
                    pos.SetValue(new DHTNode ( id, BMethods.Xor(myId, BMethods.ConvertHexToByte(id)), BMethods.BytesToString(BMethods.ConvertHexToByte(id)), BMethods.BytesToString(BMethods.Xor(myId, BMethods.ConvertHexToByte(id))) ));
                    pos.SetNext(pos2);
                    index++;
                    return true;
                }
                if (result2 == 0)
                    return true;
                pos.SetNext(new Node<DHTNode>(new DHTNode ( id, BMethods.Xor(myId, BMethods.ConvertHexToByte(id)), BMethods.BytesToString(BMethods.ConvertHexToByte(id)), BMethods.BytesToString(BMethods.Xor(myId, BMethods.ConvertHexToByte(id))) )));
                index++;
                return true;
            }
        }
        public Bucket split(int range)
        {
            int count = 0;
            Node<DHTNode> pos = bucket;
            if(pos.GetValue().XorString[range]=='0')
            {
                this.bucket = null;
                return new Bucket(pos,index);
            }
            while(pos.GetNext()!=null)
            {
                if(pos.GetNext().GetValue().XorString[range]== '0')
                {
                    Node<DHTNode> pos2 = pos.GetNext();
                    pos.SetNext(null);
                    this.index = count;
                    return new Bucket(pos2, index - count);
                }
                count++;
                pos = pos.GetNext();
            }
            return new Bucket();
        }
        public override string ToString()
        {
            Node<DHTNode> pos = bucket;
            string text = "";
            while (pos != null)
            {
                text += pos.GetValue().Bstring+"\n ";
                pos = pos.GetNext();
            }
            return text;
        }
        
    }
}