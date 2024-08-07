using System;
using System.Collections.Generic;
using System.IO;
using FileFormats.Omf;

namespace Disassembler;

public static class OmfLoader
{
    /// <summary>
    /// Loads a LIB file from disk.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static ObjectLibrary LoadLibrary(string fileName)
    {
        if (fileName == null)
            throw new ArgumentNullException(nameof(fileName));

        using var stream = File.OpenRead(fileName);
        using var reader = new BinaryReader(stream);
        var library = LoadLibrary(reader);
        library.AssignIdsToSegments();
        library.FileName = fileName;
        return library;
    }

    /// <summary>
    /// Loads a LIB file from a BinaryReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static ObjectLibrary LoadLibrary(BinaryReader reader)
    {
        if (reader == null)
            throw new ArgumentNullException(nameof(reader));

        ObjectLibrary library = new();

        foreach (var context in FileFormats.Omf.OmfLoader.LoadLibrary(reader))
        {
            var module = LoadObject(context);
            library.Modules.Add(module);
        }
        return library;
    }

    public static ObjectLibrary LoadObject(string fileName)
    {
        if (fileName == null)
            throw new ArgumentNullException(nameof(fileName));

        using var stream = File.OpenRead(fileName);
        using var reader = new BinaryReader(stream);
        ObjectLibrary library = new();

        var context = FileFormats.Omf.OmfLoader.LoadObject(reader);
        var module = LoadObject(context);
        library.Modules.Add(module);

        library.AssignIdsToSegments();
        library.FileName = fileName;
        return library;
    }

    /// <summary>
    /// Returns null if LibraryEnd record is encountered before
    /// MODEND or MODEND32 record is encountered.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static ObjectModule LoadObject(FileFormats.Omf.Records.RecordContext context)
    {
        var module = new ObjectModule();

        Dictionary<object, object> objectMap = [];

        // Convert meta-data.
        module.Name = context.ObjectName;
        module.SourceName = context.SourceName;

        // Convert segments.
        foreach (var def in context.Segments)
        {
            var segment = ConvertSegmentDefinition(def, objectMap, module);
            objectMap[def] = segment;
            module.Segments.Add(segment);
        }

        // Convert segment groups.
        foreach (var def in context.Groups)
        {
            var group = ConvertGroupDefinition(def, objectMap);
            module.Groups.Add(group);
            objectMap[def] = group;
        }

        // Convert external names.
        foreach (var def in context.ExternalNames)
        {
            var symbol = new ExternalSymbol
            {
                Name = def.Name,
                TypeIndex = def.TypeIndex,
            };
            module.ExternalNames.Add(symbol);
            objectMap[def] = symbol;
        }

        // Convert aliases.
        foreach (var def in context.Aliases)
        {
            module.Aliases.Add(new SymbolAlias
            {
                AliasName = def.AliasName,
                SubstituteName = def.SubstituteName
            });
        }

        // Convert public names.
        foreach (var def in context.PublicNames)
        {
            module.DefinedNames.Add(ConvertPublicNameDefinition(def, objectMap));
        }

        // Convert fixups.
        foreach (var def in context.Segments)
        {
            var segment = objectMap[def] as LogicalSegment;
            foreach (var f in def.Fixups)
            {
                segment.Fixups.Add(ConvertFixupDefinition(f, objectMap));
            }
        }

        return module;
    }

    private static LogicalSegment ConvertSegmentDefinition(
        SegmentDefinition def, Dictionary<object, object> objectMap,
        ObjectModule module) =>
        // Convert the record.
        new (def, objectMap, module);

    private static Fixup ConvertFixupDefinition(
        FixupDefinition def, Dictionary<object, object> objectMap)
    {
        Fixup fixup = new()
        {
            StartIndex = def.DataOffset,
            LocationType = def.Location switch
            {
                FixupLocation.LowByte => FixupLocationType.LowByte,
                FixupLocation.Offset or FixupLocation.LoaderResolvedOffset => FixupLocationType.Offset,
                FixupLocation.Base => FixupLocationType.Base,
                FixupLocation.Pointer => FixupLocationType.Pointer,
                _ => throw new InvalidDataException("The fixup location is not supported."),
            },
            Mode = def.Mode
        };

        var referent = def.Target.Referent is UInt16 v ? new PhysicalAddress(v, 0) : (IAddressReferent)objectMap[def.Target.Referent];
        fixup.Target = new SymbolicTarget
        {
            Referent = (IAddressReferent)referent,
            Displacement = def.Target.Displacement
        };
        //f.Frame = null;
        return fixup;
    }

    private static SegmentGroup ConvertGroupDefinition(
        GroupDefinition def, Dictionary<object, object> objectMap)
    {
        SegmentGroup group = new()
        {
            Name = def.Name,
            Segments = new LogicalSegment[def.Segments.Count]
        };
        for (int i = 0; i < group.Segments.Length; i++)
        {
            group.Segments[i] = (LogicalSegment)objectMap[def.Segments[i]];
        }
        return group;
    }

    private static DefinedSymbol ConvertPublicNameDefinition(
        PublicNameDefinition def, Dictionary<object, object> objectMap) => new()
        {
            BaseFrame = def.BaseFrame,
            Name = def.Name,
            TypeIndex = def.TypeIndex,
            Offset = (uint)def.Offset,
            Scope = def.IsLocal ? SymbolScope.Private : SymbolScope.Public,
            BaseGroup = def.BaseGroup != null ? objectMap[def.BaseGroup] as SegmentGroup : null,
            BaseSegment = def.BaseSegment != null ? objectMap[def.BaseSegment] as LogicalSegment : null
        };
}
