using System;
using System.Collections;
using System.Collections.Generic;

namespace Disassembler;

public class PriorityQueue<T> : Queue<T>
{
    private Func<XRef, XRef, int> compareByPriority;

    public PriorityQueue(Func<XRef, XRef, int> compareByPriority)
    {
        this.compareByPriority = compareByPriority;
    }

    public bool IsEmpty { get; internal set; }

    public new XRef Dequeue()
    {
        throw new NotImplementedException();
    }

    public void Enqueue(XRef xRef)
    {
        throw new NotImplementedException();
    }
}