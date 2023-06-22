using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class BTree<T>
    {
        private T value;
        private BTree<T> right;
        private BTree<T> left;

        public BTree(T value)
        {
            this.value = value;
            this.right = null;
            this.left = null;

        }
        public BTree(T value, BTree<T> right)
        {
            this.value = value;
            this.right = right;
            this.left = null;
        }
        public BTree(BTree<T> left, T value, BTree<T> right)
        {
            this.value = value;
            this.right = right;
            this.left = left;
        }
        public void SetValue(T value)
        {
            this.value = value;
        }
        public void SetRight(BTree<T> right)
        {
            this.right = right;
        }
        public void SetLeft(BTree<T> left)
        {
            this.left = left;
        }
        public T GetValue()
        {
            return this.value;
        }
        public BTree<T> GetRight()
        {
            return this.right;
        }
        public BTree<T> GetLeft()
        {
            return this.left;
        }
        public bool HasRight()
        {
            if (this.right == null)
                return false;
            return true;
        }
        public bool HasLeft()
        {
            if (this.left == null)
                return false;
            return true;
        }
        public override string ToString()
        {
            if (this != null)
                return $"{this.value},{this.right},{this.left}";
            else
                return "";
        }
    }
}