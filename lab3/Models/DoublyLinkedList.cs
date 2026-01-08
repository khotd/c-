using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab3.Collections
{
    public class DoublyLinkedList<T> : IList<T>, ICollection<T>, IEnumerable<T>
    {
        private class Node
        {
            public T Value;
            public Node Previous;
            public Node Next;
            public Node(T value) => Value = value;
        }

        private Node _head;
        private Node _tail;
        private int _count;

        public T this[int index]
        {
            get => GetNodeAt(index).Value;
            set => GetNodeAt(index).Value = value;
        }

        public int Count => _count;
        public bool IsReadOnly => false;

        public void Add(T item) => AddLast(item);
        
        public void AddLast(T item)
        {
            Node node = new Node(item);
            if (_tail == null) _head = _tail = node;
            else
            {
                _tail.Next = node;
                node.Previous = _tail;
                _tail = node;
            }
            _count++;
        }

        public void AddFirst(T item)
        {
            Node node = new Node(item);
            if (_head == null) _head = _tail = node;
            else
            {
                node.Next = _head;
                _head.Previous = node;
                _head = node;
            }
            _count++;
        }

        public void Clear()
        {
            _head = _tail = null;
            _count = 0;
        }

        public bool Contains(T item) => IndexOf(item) >= 0;

        public void CopyTo(T[] array, int arrayIndex)
        {
            Node current = _head;
            while (current != null)
            {
                array[arrayIndex++] = current.Value;
                current = current.Next;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            Node current = _head;
            while (current != null)
            {
                yield return current.Value;
                current = current.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(T item)
        {
            Node current = _head;
            int index = 0;
            while (current != null)
            {
                if (EqualityComparer<T>.Default.Equals(current.Value, item))
                    return index;
                current = current.Next;
                index++;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            if (index < 0 || index > _count) throw new IndexOutOfRangeException();
            
            if (index == 0) AddFirst(item);
            else if (index == _count) AddLast(item);
            else
            {
                Node current = GetNodeAt(index);
                Node newNode = new Node(item);
                
                newNode.Previous = current.Previous;
                newNode.Next = current;
                current.Previous.Next = newNode;
                current.Previous = newNode;
                _count++;
            }
        }

        public bool Remove(T item)
        {
            Node current = _head;
            while (current != null)
            {
                if (EqualityComparer<T>.Default.Equals(current.Value, item))
                {
                    RemoveNode(current);
                    return true;
                }
                current = current.Next;
            }
            return false;
        }

        public void RemoveAt(int index) => RemoveNode(GetNodeAt(index));
        
        private Node GetNodeAt(int index)
        {
            if (index < 0 || index >= _count) throw new IndexOutOfRangeException();
            
            if (index < _count / 2)
            {
                Node current = _head;
                for (int i = 0; i < index; i++) current = current.Next;
                return current;
            }
            else
            {
                Node current = _tail;
                for (int i = _count - 1; i > index; i--) current = current.Previous;
                return current;
            }
        }

        private void RemoveNode(Node node)
        {
            if (node.Previous != null) node.Previous.Next = node.Next;
            else _head = node.Next;
            
            if (node.Next != null) node.Next.Previous = node.Previous;
            else _tail = node.Previous;
            
            _count--;
        }

        public void RemoveFirst()
        {
            if (_head == null)
                throw new InvalidOperationException("Список пуст");
            RemoveNode(_head);
        }

        public void RemoveLast()
        {
            if (_tail == null)
                throw new InvalidOperationException("Список пуст");
            RemoveNode(_tail);
        }
    }
}