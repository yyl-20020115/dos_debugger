﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler
{
    public class ControlFlowGraph
    {
        readonly XRefCollection graph = new XRefCollection();
        readonly BasicBlockCollection blocks;

        public ControlFlowGraph(BasicBlockCollection collection)
        {
            this.blocks = collection;
        }

        public void AddEdge(BasicBlock source, BasicBlock target, XRef xref)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null)
                throw new ArgumentNullException("target");
            if (xref == null)
                throw new ArgumentNullException("xref");

            System.Diagnostics.Debug.Assert(blocks.Contains(source));
            System.Diagnostics.Debug.Assert(blocks.Contains(target));

            XRef xFlow = new XRef(
                type: xref.Type,
                source: source.Location,
                target: target.Location,
                dataLocation: xref.Source
            );
            graph.Add(xFlow);
        }

        public ICollection<XRef> Edges
        {
            get { return graph; }
        }

        public IEnumerable<BasicBlock> GetSuccessors(BasicBlock source)
        {
            foreach (XRef xref in graph.GetReferencesFrom(source.Location))
            {
                // TODO: change Find to ExactMatch.
                yield return blocks.Find(xref.Target);
            }
        }

        public XRefCollection Graph
        {
            get { return graph; }
        }
    }
}
