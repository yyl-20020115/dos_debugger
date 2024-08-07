﻿using System;
using System.Collections.Generic;
//using Util;

namespace Disassembler
{
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
    public class Error
    {
        readonly Address location;
        readonly ErrorCode errorCode;
        readonly string message;

        public Error(Address location, ErrorCode errorCode, string message)
        {
            this.location = location;
            this.errorCode = errorCode;
            this.message = message;
        }

        public ErrorCode ErrorCode
        {
            get { return errorCode; }
        }

        public ErrorCategory Category
        {
            get
            {
                var attribute = errorCode.GetAttribute<ErrorCategoryAttribute>();
                if (attribute != null)
                    return attribute.Category;
                else
                    return ErrorCategory.Error;
            }
        }

        public Address Location
        {
            get { return location; }
        }

        public string Message
        {
            get { return message; }
        }

        public static int CompareByLocation(Error x, Error y)
        {
            return x.Location.CompareTo(y.Location);
        }
    }

    [Flags]
    public enum ErrorCategory
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Message = 4,
    }

    public class ErrorCategoryAttribute : Attribute
    {
        readonly ErrorCategory category;

        public ErrorCategoryAttribute(ErrorCategory category)
        {
            this.category = category;
        }

        public ErrorCategory Category
        {
            get { return category; }
        }
    }

    public enum ErrorCode
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
        readonly List<Error> errors = new List<Error>();

        public void Add(Error error)
        {
            errors.Add(error);
        }

        public void Clear()
        {
            errors.Clear();
        }

        public bool Contains(Error item)
        {
            return errors.Contains(item);
        }

        public void CopyTo(Error[] array, int arrayIndex)
        {
            errors.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return errors.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Error item)
        {
            return errors.Remove(item);
        }

        public IEnumerator<Error> GetEnumerator()
        {
            return errors.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
