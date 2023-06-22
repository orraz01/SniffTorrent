using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Node<T>
    {
        private T value;
        private Node<T> next;

        public Node(T value)
        {
            this.value = value;
            this.next = null;
        }
        public Node(T value, Node<T> next)
        {
            this.value = value;
            this.next = next;
        }
        public void SetValue(T value)
        {
            this.value = value;
        }
        public void SetNext(Node<T> next)
        {
            this.next = next;
        }
        public T GetValue()
        {
            return this.value;
        }
        public Node<T> GetNext()
        {
            return this.next;
        }
        public bool Remove(T value)
        {
            Node<T> pos = this;
            while(pos.next.next!=null)
            {
                if(value is T)
                {
                    if(value.Equals(pos.GetValue()))
                    {
                        pos.value = pos.next.value;
                        pos.next = pos.next.next;
                        return true;
                    }
                }
                pos = pos.next;
            }
            if (value.Equals(pos.GetValue()))
            {
                pos.value = pos.next.value;
                pos.next = pos.next.next;
                return true;
            }
            else if((value.Equals(pos.next.GetValue())))
            {
                pos.next = null;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"{value},{next}";
        }
    }
}