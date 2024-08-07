using System;

namespace Disassembler
{
    internal class PriorityQueue<T>
    {
        private Func<XRef, XRef, int> compareByPriority;

        public PriorityQueue(Func<XRef, XRef, int> compareByPriority)
        {
            this.compareByPriority = compareByPriority;
        }

        public bool IsEmpty { get; internal set; }

        internal XRef Dequeue()
        {
            throw new NotImplementedException();
        }

        internal void Enqueue(XRef xRef)
        {
            throw new NotImplementedException();
        }
    }
}