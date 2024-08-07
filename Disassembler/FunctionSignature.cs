using System;

namespace Disassembler;

/// <summary>
/// Contains information about the (low-level) signature of a function.
/// </summary>
public class FunctionSignature
{
    /// <summary>
    /// Gets or sets the name of the function.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the calling convention of the function.
    /// </summary>
    public CallingConvention CallingConvention { get; set; }

    /// <summary>
    /// Gets or sets the total number of bytes of parameters, including
    /// those passed in registers. If this information is not available
    /// (such as in cdecl), the value is -1.
    /// </summary>
    public int ParametersSize { get; set; }
}

/// <summary>
/// Defines the calling convention of a function.
/// </summary>
/// <remarks>
/// For a detailed list of the calling conventions of different compilers
/// and systems, see http://en.wikipedia.org/wiki/X86_calling_conventions.
/// </remarks>
public enum CallingConvention : int
{
    Unknown = 0,

    /// <summary>
    /// Parameters passed on the stack from right to left; caller cleans
    /// the stack.
    /// </summary>
    CDecl = 1,

    /// <summary>
    /// Parameters passed on the stack from left to right; callee cleans
    /// the stack. This convention is mainly used on 16-bit systems.
    /// </summary>
    Pascal = 2,

    /// <summary>
    /// Parameters passed in AX, DX, and then pushed to the stack LTR.
    /// The return address is passed in BX. Callee cleans the stack.
    /// </summary>
    FastCall = 3,
}

public static class NameMangler
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="decoration"></param>
    /// <returns></returns>
    /// <remarks>
    /// The name decoration of a C function is described at
    /// http://en.wikipedia.org/wiki/Name_mangling#C_name_decoration_in_Microsoft_Windows
    /// </remarks>
    public static FunctionSignature Demangle(string decoration)
    {
        if (decoration == null)
            throw new ArgumentNullException(nameof(decoration));
        if (decoration.Length <= 1)
            return null;

        if (decoration[0] == '_')
        {
            int at = decoration.IndexOf('@');
            if (at == -1) // _f
            {
                return new ()
                {
                    CallingConvention = CallingConvention.CDecl,
                    Name = decoration.Substring(1),
                    ParametersSize = -1
                };
            }

            if (int.TryParse(decoration.Substring(at + 1), out int paramSize)) // _f@4
            {
                return new ()
                {
                    CallingConvention = CallingConvention.Pascal,
                    Name = decoration.Substring(1, at - 1),
                    ParametersSize = paramSize
                };
            }

            return null;
        }

        if (decoration[0] == '@') // @f@4 -- fast call
        {
            int at = decoration.IndexOf('@', 1);
            if (at >= 0)
            {
                if (int.TryParse(decoration.Substring(at + 1), out int paramSize))
                {
                    return new ()
                    {
                        CallingConvention = CallingConvention.FastCall,
                        Name = decoration.Substring(1, at - 1),
                        ParametersSize = paramSize
                    };
                }
            }
            return null;
        }

        if (decoration[0] == '?')
        {
            return DemangleCpp(decoration);
        }

        return null;
    }

    /// <remarks>
    /// The name mangling method of VC++ is described at
    /// http://en.wikipedia.org/wiki/Visual_C%2B%2B_name_mangling
    /// </remarks>
    private static FunctionSignature DemangleCpp(string decoration)
    {
        return null;
    }
}
