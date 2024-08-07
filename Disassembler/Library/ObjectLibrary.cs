using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Disassembler;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class ObjectLibrary : Assembly
{
#if false
    public ObjectLibrary(IEnumerable<ObjectModule> modules)
    {
        if (modules == null)
            throw new ArgumentNullException("modules");

        foreach (ObjectModule module in modules)
            base.Modules.Add(module);
    }
#endif

#if false
    /// <summary>
    /// Gets a list of object modules in this library.
    /// </summary>
    //[TypeConverter(typeof(ExpandableObjectConverter))]
    //[TypeConverter(typeof(ArrayConverter))]
    //[TypeConverter(typeof(CollectionConverter))]
    //[TypeConverter(typeof(ExpandableCollectionConverter))]
    [Browsable(true)]
    public ObjectModule[] Modules { get; internal set; }
#endif

    public string FileName { get; set; }

    public string Name => System.IO.Path.GetFileName(FileName);

    public readonly SortedDictionary<string, List<ObjectModule>> Symbols
        = [];

    public LibraryImage Image { get; set; }

    public override BinaryImage GetImage() => Image;

    public IEnumerable<string> GetUnresolvedSymbols()
    {
        foreach (var kv in Symbols)
        {
            if (kv.Value == null)
                yield return kv.Key;
        }
    }

    public ObjectModule FindModule(string name)
    {
        if (name == null)
            throw new ArgumentNullException("name");

        foreach (ObjectModule module in Modules.Cast<ObjectModule>())
        {
            if (module.Name == name)
                return module;
        }
        return null;
    }

    public void AssignIdsToSegments()
    {
        this.Image = new LibraryImage();
        foreach (ObjectModule module in Modules.Cast<ObjectModule>())
        {
            foreach (LogicalSegment segment in module.Segments)
            {
                segment.Id = Image.Segments.Count;
                Image.Segments.Add(new LibrarySegment(segment));
            }
        }
    }

    public void ResolveAllSymbols()
    {
        Dictionary<string, DefinedSymbol> publicNames = [];

        // First, build a map of each public name.
        foreach (ObjectModule module in Modules.Cast<ObjectModule>())
        {
            foreach (DefinedSymbol name in module.DefinedNames)
            {
                if (!Symbols.TryGetValue(name.Name, out List<ObjectModule> definitionList))
                {
                    definitionList = new List<ObjectModule>(1);
                    Symbols.Add(name.Name, definitionList);
                }
                definitionList.Add(module);
                publicNames[name.Name] = name;
            }
        }

        // Next, try to resolve each external symbol.
        // TODO: check aliases.
        foreach (ObjectModule module in Modules.Cast<ObjectModule>())
        {
            foreach (var name in module.ExternalNames)
            {
                if (!Symbols.ContainsKey(name.Name)) // cannot resolve
                {
                    Symbols.Add(name.Name, null); // indicate that it's not there
                }
                if (publicNames.ContainsKey(name.Name))
                {
                    name.ResolvedAddress = publicNames[name.Name].ResolvedAddress;
                }
            }
        }
    }
}
