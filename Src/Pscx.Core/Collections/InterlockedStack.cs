using System.Threading;

namespace Pscx.Collections
{
    public sealed class InterlockedStack<T>
    {
        private readonly Node _head = new Node(default(T));

        public InterlockedStack()
        {
        }

        public void Push(T item)
        {
            Node node = new Node(item);

            do
            {
                node.Next = _head.Next;
            }
            while (!CompareAndSwitch(ref _head.Next, node.Next, node));
        }

        public T Pop()
        {
            Node node;

            do
            {
                node = _head.Next;

                if (node == null)
                {
                    return default(T);
                }
            }
            while (!CompareAndSwitch(ref _head.Next, node, node.Next));

            return node.Value;
        }

        private static bool CompareAndSwitch(ref Node location, Node comparand, Node newValue)
        {
            return comparand == Interlocked.CompareExchange<Node>(ref location, newValue, comparand);
        }

        private class Node
        {
            public Node(T value)
            {
                Value = value;
            }

            public Node Next;
            public readonly T Value;
        }
    }
}
