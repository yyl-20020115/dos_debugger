using System;
using System.Collections;
using System.Collections.Generic;

namespace Disassembler;

public class PriorityQueue<T>(Func<T, T, int> compareByPriority) : Queue<T>
{
    private readonly Func<T, T, int> compareByPriority = compareByPriority;

    public bool IsEmpty => this.Count == 0;

   

}