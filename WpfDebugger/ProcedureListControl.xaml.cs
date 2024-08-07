using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using Disassembler;

namespace WpfDebugger;

/// <summary>
/// Interaction logic for ProcedureListControl.xaml
/// </summary>
public partial class ProcedureListControl : UserControl
{
    private Assembly program;

    public ProcedureListControl()
    {
        InitializeComponent();
    }

    public Assembly Program
    {
        get => program;
        set
        {
            program = value;
            if (program == null)
                this.DataContext = null;
            else
                this.DataContext = new ProcedureListViewModel(program);
        }
    }

    #region Navigation

    private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListViewItem)
            return;

        if (((ListViewItem)sender).Content is not ProcedureListViewModel.ProcedureItem item)
            return;

        Uri uri = item.Uri;
        var targetName = GetTargetNameFromModifierKeys();
        RaiseRequestNavigate(uri, targetName);
    }

    private static string GetTargetNameFromModifierKeys() => Keyboard.Modifiers switch
    {
        ModifierKeys.Control => "asm:_blank",
        ModifierKeys.Shift => "hex",
        ModifierKeys.Control | ModifierKeys.Shift => "hex:_blank",
        _ => "asm",
    };

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        var hyperlink = sender as Hyperlink;
        if (hyperlink != null)
        {
            // Select the containing ListViewItem.
            var item = lvProcedures.ItemContainerGenerator.ContainerFromItem(
                hyperlink.DataContext) as ListViewItem;
            if (item != null)
            {
                item.IsSelected = true;
                item.Focus();
            }

            // Raise RequestNavigate event.
            var uri = hyperlink.NavigateUri;
            var targetName = GetTargetNameFromModifierKeys();
            RaiseRequestNavigate(uri, targetName);
        }
    }

#if false
    private static T FindParent<T>(DependencyObject element)
        where T : DependencyObject
    {
        while (element != null)
        {
            element = System.Windows.Media.VisualTreeHelper.GetParent(element);
            if (element is T)
                return (T)element;
        }
        return null;
    }
#endif

    private void RaiseRequestNavigate(Uri uri, string targetName)
    {
        if (RequestNavigate != null && uri != null)
        {
            var e = new RequestNavigateEventArgs(uri, targetName);
            RequestNavigate(this, e);
        }
    }

    private void ListView_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (lvProcedures.SelectedItem is not ProcedureListViewModel.ProcedureItem item)
                return;

            e.Handled = true;
            var uri = item.Uri;
            var targetName = GetTargetNameFromModifierKeys();
            RaiseRequestNavigate(uri, targetName);
        }
    }

    private void OpenLinkCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void OpenDisassemblyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        RaiseRequestNavigate(e.Parameter as Uri, "asm");
    }

    private void OpenNewDisassemblyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        RaiseRequestNavigate(e.Parameter as Uri, "asm:_blank");
    }

    private void OpenHexViewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        RaiseRequestNavigate(e.Parameter as Uri, "hex");
    }

    private void OpenNewHexViewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        RaiseRequestNavigate(e.Parameter as Uri, "hex:_blank");
    }

    public event EventHandler<RequestNavigateEventArgs> RequestNavigate;

    #endregion
}

class ProcedureListViewModel
{
    public List<ProcedureItem> Items { get; private set; }

    public ProcedureListViewModel(Assembly program)
    {
        this.Items = [];
        var list = from proc in program.GetImage().Procedures
                   orderby proc.EntryPoint
                   select proc;
        foreach (Procedure proc in list)
        {
            ProcedureItem viewItem = new ProcedureItem(program, proc);
            Items.Add(viewItem);
        }
    }

    public class ProcedureItem(Assembly program, Procedure procedure)
    {
        readonly Assembly program = program ?? throw new ArgumentNullException(nameof(program));

        public Procedure Procedure { get; } = procedure ?? throw new ArgumentNullException(nameof(procedure));

        public string Name => Procedure.Name;

        public Address EntryPoint => Procedure.EntryPoint;

        public string EntryPointString => program.GetImage().FormatAddress(Procedure.EntryPoint);

        public int Size => Procedure.Size;

        public Uri Uri => new AssemblyUri(program, Procedure.EntryPoint);
    }
}
