using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using Disassembler;

namespace WpfDebugger;

/// <summary>
/// Interaction logic for LibraryBrowserControl.xaml
/// </summary>
public partial class LibraryBrowserControl : UserControl
{
    public LibraryBrowserControl()
    {
        InitializeComponent();
#if false
    typeof(VirtualizingStackPanel).GetProperty("IsPixelBased", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, true, null);
#endif
    }

    private ObjectLibrary library;

    public ObjectLibrary Library
    {
        get => library;
        set
        {
            library = value;
            if (library == null)
            {
                this.DataContext = null;
            }
            else
            {
                var viewModel = new LibraryBrowserViewModel(library);
                this.DataContext = viewModel;
                myTreeView.ItemsSource = viewModel.Libraries;
            }
        }
    }

    private void TreeView_ItemActivate(object sender, EventArgs e)
    {
        if (GetObjectFromItem(sender) != null)
            ActivateObject(GetObjectFromItem(sender));
    }

    private void ActivateObject(object obj)
    {
        if (obj is LogicalSegment segment)
        {
            DisassembleSegment(segment, 0);
        }
        else if (obj is DefinedSymbol symbol)
        {
            if (symbol.BaseSegment != null &&
                symbol.BaseSegment.Class.EndsWith("CODE"))
            {
                DisassembleSegment(symbol.BaseSegment, (int)symbol.Offset);
            }
        }
#if false
        else if (sender is LibraryBrowserViewModel.ModuleItem)
        {
            // Disassemble the first code segment.
            var module = ((LibraryBrowserViewModel.ModuleItem)sender).Module;
            LogicalSegment segment = module.Segments.FirstOrDefault(
                s => s.Class.EndsWith("CODE"));
            if (segment != null)
            {
                DisassembleSegment(segment, module);
            }
        }
#endif
    }

    private void DisassembleSegment(LogicalSegment segment, int offset)
    {
        // Raise request navigate event.
        if (this.RequestNavigate != null)
        {
            AssemblyUri uri = new (library, segment, offset);
            var e = new RequestNavigateEventArgs(uri, null);
            this.RequestNavigate(this, e);
        }
    }

    private void TreeView_SelectionChanged(object sender, EventArgs e)
    {
        object obj = GetObjectFromItem(sender);

        if (RequestProperty != null && obj != null)
            RequestProperty(this, new RequestPropertyEventArgs(obj));

        if (obj != null)
            ActivateObject(obj);
    }

    private static object GetObjectFromItem(object sender) => sender switch
    {
        LibraryBrowserViewModel.LibraryItem => ((LibraryBrowserViewModel.LibraryItem)sender).Library,
        LibraryBrowserViewModel.ModuleItem => ((LibraryBrowserViewModel.ModuleItem)sender).Module,
        LibraryBrowserViewModel.SymbolItem => ((LibraryBrowserViewModel.SymbolItem)sender).Symbol,
        LibraryBrowserViewModel.SymbolAliasItem => ((LibraryBrowserViewModel.SymbolAliasItem)sender).Alias,
        LibraryBrowserViewModel.SegmentItem => ((LibraryBrowserViewModel.SegmentItem)sender).Segment,
        _ => null,
    };

    public event EventHandler<RequestPropertyEventArgs> RequestProperty;
    public event EventHandler<RequestNavigateEventArgs> RequestNavigate;
}

public class RequestPropertyEventArgs(object selectedObject) : EventArgs
{
    public object SelectedObject { get; private set; } = selectedObject;
}

internal class LibraryBrowserViewModel(ObjectLibrary library)
{
    public LibraryItem[] Libraries { get; private set; } = [new (library)];
    public LibraryItem Library { get { return Libraries[0]; } }

    internal class LibraryItem(ObjectLibrary library) : ITreeNode
    {
        public ObjectLibrary Library { get; private set; } = library ?? throw new ArgumentNullException(nameof(library));
        public ObservableCollection<ModuleItem> Modules { get; private set; } =
                new ObservableCollection<ModuleItem>(
                    from ObjectModule module in library.Modules
                    orderby module.Name
                    select new ModuleItem(module));
        public string Name => "Library";

        public string Text => "Library";

        public string ImageKey => "LibraryImage";

        public bool HasChildren => Modules.Count > 0;

        public IEnumerable<ITreeNode> GetChildren() => Modules;
    }

    internal class ModuleItem : ITreeNode
    {
        public ObjectModule Module { get; private set; }
        public string Name => Module.Name ?? "(" + Module.SourceName + ")";
        public List<ITreeNode> Symbols { get; private set; }

        public ModuleItem(ObjectModule module)
        {
            this.Module = module ?? throw new ArgumentNullException(nameof(module));
            this.Symbols = [];

            ConstantSegmentItem constantGroup = new(module);
            if (constantGroup.HasChildren)
                this.Symbols.Add(constantGroup);

            // Hide zero-length segments.
            this.Symbols.AddRange(
                from segment in module.Segments
                where segment.Length > 0
                orderby segment.Class, segment.Name
                select new SegmentItem(segment, module));

            this.Symbols.AddRange(
                from alias in module.Aliases
                select (ITreeNode)new SymbolAliasItem(alias));

#if false
            this.Symbols.AddRange(
                from symbol in module.DefinedNames
                where symbol.BaseSegment != null
                orderby symbol.BaseSegment.Name, symbol.Offset, symbol.Name
                select new SymbolItem(symbol));
#endif
        }

        public string Text => this.Name;

        public string ImageKey => "ModuleImage";

        public bool HasChildren => Symbols.Count > 0;

        public IEnumerable<ITreeNode> GetChildren() => Symbols;
    }

    internal class ConstantSegmentItem(ObjectModule module) : ITreeNode
    {
        private readonly ObjectModule module = module;
        private readonly List<SymbolItem> constants = new(
                from symbol in module.DefinedNames
                where symbol.BaseSegment == null
                orderby symbol.Name
                select new SymbolItem(symbol));

        public string Text => "Constants";

        public string ImageKey => "SegmentImage";

        public bool HasChildren => constants.Count > 0;

        public IEnumerable<ITreeNode> GetChildren() => constants;
    }

    internal class SegmentItem(LogicalSegment segment, ObjectModule module) : ITreeNode
    {
        readonly LogicalSegment segment = segment;
        readonly List<SymbolItem> symbols = new List<SymbolItem>(
                from symbol in module.DefinedNames
                where symbol.BaseSegment == segment
                orderby symbol.Offset, symbol.Name
                select new SymbolItem(symbol));

        public LogicalSegment Segment => segment;

        public string Text => $"{segment.Class}: {segment.Name} [{segment.Length}]";

        public string ImageKey =>
                // if (segment.Combination == Private), return private;
                "SegmentImage";

        public bool HasChildren => symbols.Count > 0;

        public IEnumerable<ITreeNode> GetChildren() => symbols;
    }

    internal class SymbolItem(DefinedSymbol symbol) : ITreeNode
    {
        public DefinedSymbol Symbol { get; private set; } = symbol ?? throw new ArgumentNullException(nameof(symbol));

        public override string ToString() => Symbol.BaseSegment == null
                ? $"{Symbol.BaseFrame:X4}:{Symbol.Offset:X4}  {Symbol.Name}"
                : $"{Symbol.BaseSegment.Name}+{Symbol.Offset:X4}  {Symbol.Name}";

        public string Text => this.ToString();

        public string ImageKey
        {
            get
            {
                if (Symbol.BaseSegment == null)
                {
                    // An absolute symbol is typically used to store
                    // a constant.
                    return "ConstantImage";
                }

                string className = Symbol.BaseSegment.Class;
                if (className.EndsWith("CODE"))
                {
                    if (Symbol.Scope == SymbolScope.Private)
                        return "LocalProcedureImage";
                    else
                        return "ProcedureImage";
                }
                else if (className.EndsWith("DATA"))
                {
                    if (Symbol.Scope == SymbolScope.Private)
                        return "LocalFieldImage";
                    else
                        return "FieldImage";
                }
                else
                    return null;
            }
        }

        public bool HasChildren => false;

        public IEnumerable<ITreeNode> GetChildren() => null;
    }

    internal class SymbolAliasItem(SymbolAlias alias) : ITreeNode
    {
        public SymbolAlias Alias { get; private set; } = alias ?? throw new ArgumentNullException(nameof(alias));

        public override string ToString() => $"{Alias.Name} -> {Alias.SubstituteName}";

        public string Text => this.ToString();

        public string ImageKey => "ProcedureAliasImage";

        public bool HasChildren => false;

        public IEnumerable<ITreeNode> GetChildren() => null;
    }
}
