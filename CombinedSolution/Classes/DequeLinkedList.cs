using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombinedSolution.Classes
{
    internal class DequeLinkedList<T>
    {
        private Node<T> _head;
        private Node<T> _tail;
        private int _count;
        private readonly object _locker;
        public DequeLinkedList()
        {
            _locker = new object();
            _head = null;
            _tail = null;
            _count = 0;
        }
        public bool Push(T value)
        {
            bool retVal = false;
            Node<T> node = null;
            if (value != null)
            {
                lock (_locker)
                {
                    if (_head == null) // First node
                    {
                        node = new Node<T>(value, null, null);
                        _head = node;
                        _tail = node;
                        ++_count;
                        retVal = true;
                    }
                    else
                    {
                        node = new Node<T>(value, _head, null);
                        _head.AddAfter(node);
                        _head = node;
                        ++_count;
                        retVal = true;
                    }
                }
            }
            return retVal;
        }
        public T Pop()
        {
            T retVal = default(T);
            lock (_locker)
            {
                if (_head != null)
                {
                    retVal = _head.Value;
                    Node<T> temp = _head.PreviousNode;
                    _head.Delete();
                    if (temp != null)
                    {
                        temp.AddAfter(null);
                        _head = temp;
                    }
                }
            }
            return retVal;
        }
        public bool Inject(T value)
        {
            bool retVal = false;
            Node<T> node = null;
            if (value != null)
            {
                lock (_locker)
                {
                    if (_tail == null)
                    {
                        node = new Node<T>(value, null, null);
                        _head = node;
                        _tail = node;
                        ++_count;
                        retVal = true;
                    }
                    else
                    {
                        node = new Node<T>(value, null, _tail);
                        _tail.AddBefore(node);
                        _tail = node;
                        ++_count;
                        retVal = true;
                    }
                }
            }
            return retVal;
        }
        public T Eject()
        {
            T retVal = default(T);
            lock (_locker)
            {
                if (_tail != null)
                {
                    retVal = _tail.Value;
                    Node<T> temp = _tail.NextNode;
                    _tail.Delete();
                    if (temp != null)
                    {
                        temp.AddBefore(null);
                        _tail = temp;
                    }
                }
            }
            return retVal;
        }
        public int Count
        {
            get
            {
                int retVal = 0;
                lock (_locker)
                {
                    retVal = _count;
                }
                return retVal;
            }
        }
    }
    internal class Node<T>
    {
        private T _value;
        private Node<T> _previous;
        private Node<T> _next;

        public Node(T value, Node<T> prev, Node<T> next)
        {
            _value = value;
            _previous = prev;
            _next = next;
        }
        public void AddBefore(Node<T> val)
        {
            _previous = val;
        }
        public void AddAfter(Node<T> val)
        {
            _next = val;
        }
        public Node<T> NextNode
        {
            get
            {
                return _next;
            }
        }
        public Node<T> PreviousNode
        {
            get
            {
                return _previous;
            }
        }
        public T Value
        {
            get
            {
                return _value;
            }
        }
        public void Delete()
        {
            _value = default(T);
            _next = null;
            _previous = null;
        }
    }
}
