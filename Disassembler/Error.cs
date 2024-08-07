using System;
using System.Collections.Generic;

namespace Disassembler;

public static class AttributeUtils
{
    //TODO:
    public static Attribute GetAttribute<Attribute>(this Enum attribute)
    {
        return default;
    }

}
/// <summary>
/// Contains information about an error encountered during disassembling.
/// </summary>
public class Error(Address location, ErrorCode errorCode, string message)
{
    readonly Address location = location;
    readonly string message = message;

    public ErrorCode ErrorCode { get; } = errorCode;

    public ErrorCategory Category
    {
        get
        {
            var attribute = ErrorCode.GetAttribute<ErrorCategoryAttribute>();
            return attribute != null ? attribute.Category : ErrorCategory.Error;
        }
    }

    public Address Location => location;

    public string Message => message;

    public static int CompareByLocation(Error x, Error y) => x.Location.CompareTo(y.Location);
}

[Flags]
public enum ErrorCategory : int
{
    None = 0,
    Error = 1,
    Warning = 2,
    Message = 4,
}

public class ErrorCategoryAttribute(ErrorCategory category) : Attribute
{
    public ErrorCategory Category { get; } = category;
}

public enum ErrorCode : int
{
    [ErrorCategory(ErrorCategory.None)]
    OK = 0,

    GenericError,
    InvalidInstruction,
    BrokenFixup,

    /// <summary>
    /// Indicates that the same procedure (identified by its entry point
    /// address) was called both near and far. Since a near call must be
    /// returned with RETN and a far call with RETF, this error indicates
    /// a potential problem with the analysis.
    /// </summary>
    /// [Summary]
    /// [Description]
    /// [Example]
    /// [Solution]
    [ErrorCategory(ErrorCategory.Warning)]
    InconsistentCall,

    /// <summary>
    /// Indicates that data was encountered when code was expected.
    /// </summary>
    RanIntoData,

    /// <summary>
    /// Indicates that we ran into the middle of an instruction.
    /// </summary>
    RanIntoCode,

    /// <summary>
    /// While analyzing a basic block, a decoded instruction would 
    /// overlap with existing bytes that are already analyzed as code
    /// or data.
    /// </summary>
    /// <remarks>
    /// Possible causes for this error include:
    /// - 
    /// - Other analysis errors.
    /// </remarks>
    [ErrorCategory(ErrorCategory.Error)]
    OverlappingInstruction,

    /// <summary>
    /// After executing an instruction, the CS:IP pointer would wrap
    /// around 0xFFFF.
    /// </summary>
    /// <remarks>
    /// While it is technically allowed (and occassionally useful) to let
    /// CS:IP wrap, this situation typically indicates an analysis error.
    /// </remarks>
    [ErrorCategory(ErrorCategory.Error)]
    AddressWrapped,

    /// <summary>
    /// The target of a branch/call/jump instruction cannot be determined
    /// through static analysis.
    /// </summary>
    /// <remarks>
    /// To resolve this problem, dynamic analysis can be employed.
    /// </remarks>
    [ErrorCategory(ErrorCategory.Message)]
    DynamicTarget,

    /// <summary>
    /// The target of a branch/call/jump instruction refers to a fix-up
    /// target, but the target cannot be resolved.
    /// </summary>
    UnresolvedTarget,

    /// <summary>
    /// The target of a branch/call/jump instruction refers to a location
    /// outside of the binary image.
    /// </summary>
    OutOfImage,

    /// <summary>
    /// Indicates that a basic block ended prematurally due to an anlysis
    /// error.
    /// </summary>
    BrokenBasicBlock,

    /// <summary>
    /// Indicates that a fixup is intentionally discarded because it
    /// might not suit the purpose of the analysis. This is currently
    /// used only with floating point emulator fix-ups.
    /// </summary>
    [ErrorCategory(ErrorCategory.Warning)]
    FixupDiscarded,
}

// Note: errorcollection and fixupcollection actually have similar
// data structures, so we should find a way to merge them. However,
// a fixup collection doesn't allow multiple errors at the same
// location, but we allow them here.
public class ErrorCollection : ICollection<Error>
{
    readonly List<Error> errors = [];

    public void Add(Error error) => errors.Add(error);

    public void Clear() => errors.Clear();

    public bool Contains(Error item) => errors.Contains(item);

    public void CopyTo(Error[] array, int arrayIndex) => errors.CopyTo(array, arrayIndex);

    public int Count => errors.Count;

    public bool IsReadOnly => false;

    public bool Remove(Error item) => errors.Remove(item);

    public IEnumerator<Error> GetEnumerator() => errors.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
