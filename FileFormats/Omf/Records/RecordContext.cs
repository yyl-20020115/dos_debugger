using System.Collections.Generic;

namespace FileFormats.Omf.Records;

/// <summary>
/// Contains context information to assist reading and writing records.
/// </summary>
public class RecordContext
{
    // Populated by THEADR records.
    public string ObjectName;
    
    // Populated by COMENT/LIBMOD subrecords.
    public string SourceName;

    // Populated by LNAMES records.
    public readonly List<string> Names = [];

    // Populated by SEGDEF records.
    public readonly List<SegmentDefinition> Segments =
        [];

    // Populated by GRPDEF records.
    public readonly List<GroupDefinition> Groups =
        [];

    // Populated by EXTDEF, LEXTDEF, CEXTDEF, COMDEF, and LCOMDEF records.
    public readonly List<ExternalNameDefinition> ExternalNames =
        [];

    // Populated by PUBDEF and LPUBDEF records.
    public readonly List<PublicNameDefinition> PublicNames =
        [];

#if false
    // Populated by COMDEF and LCOMDEF records.
    public readonly List<CommunalNameDefinition> CommunalNames =
        new List<CommunalNameDefinition>();
#endif

    // Populated by ALIAS records.
    public readonly List<AliasDefinition> Aliases =
        [];

    // FRAME threads.
    internal readonly FixupThreadDefinition[] FrameThreads = new FixupThreadDefinition[4];

    // TARGET threads.
    internal readonly FixupThreadDefinition[] TargetThreads = new FixupThreadDefinition[4];

    // Contains the last record.
    internal Record LastRecord = null;

    internal Record[] Records;
}
