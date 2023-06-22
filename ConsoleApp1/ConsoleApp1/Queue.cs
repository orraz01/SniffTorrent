using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Queue<T>
    {
        private Node<T> head;
        private Node<T> tail;
        public Queue()
        {
            this.head = null;
            this.tail = null;
        }
        public void Insert(T x)
        {
            Node<T> temp = new Node<T>(x);
            if (this.head == null)
            {
                this.head = temp;
                this.tail = temp;
            }
            else
            {
                this.tail.SetNext(new Node<T>(x));
                this.tail = this.tail.GetNext();
            }

        }
        public T Remove()
        {
            T x = this.head.GetValue();
            this.head = this.head.GetNext();
            return x;
        }
        public bool IsEmpty()
        {
            if (head == null)
                return true;
            return false;
        }
        public T Head()
        {
            T x = this.head.GetValue();
            return x;
        }
        public override string ToString()
        {
            Node<T> temp = this.head;
            return $"{head}";


        }
    }
}