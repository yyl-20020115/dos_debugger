﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Disassembler;

namespace WpfDebugger;

/// <summary>
/// Interaction logic for DisassemblyControl.xaml
/// </summary>
public partial class DisassemblyControl : UserControl
{
    public DisassemblyControl()
    {
        InitializeComponent();
    }

    private ListingViewModel viewModel;

#if false
    public ImageChunk Image
    {
        get { return image; }
        set
        {
            image = value;
            UpdateUI();
        }
    }
#endif

    public void SetView(Assembly assembly, Address address)
    {
        if (viewModel == null || viewModel.Image != assembly.GetImage())
        {
            this.DataContext = null;
            this.viewModel = new (assembly, address.Segment);
            this.DataContext = viewModel;
        }
        GoToAddress(address.Offset);
    }

#if true
    public void GoToAddress(int offset)
    {
        int index = viewModel.FindRowIndex(offset);
        if (index < 0 || index >= viewModel.Rows.Count)
            return;

        // Scroll to the bottom first so that the actual item will be
        // on the top when we scroll again.
        lvListing.ScrollIntoView(viewModel.Rows[viewModel.Rows.Count - 1]);

        // We must UpdateLayout() now, otherwise the first dummy scroll
        // won't have any effect.
        lvListing.UpdateLayout();

        // Now scroll the actual item into view.
        lvListing.ScrollIntoView(viewModel.Rows[index]);

        // TBD: The first time we scroll, it won't actually scroll to the
        // location, even if we call UpdateLayout() again. Find out why.
        //lvListing.UpdateLayout();

        // Select the item.
        lvListing.SelectedIndex = index;

        // Note: we MUST get the ListViewItem and call Focus() on this
        // item. If we instead call Focus() on lvListing, the UI will
        // hang if
        //   1) the focused item is out of the screen, and
        //   2) we press Up/Down arrow.
        // The reason is probably that there is no ListViewItem created
        // for an off-the-screen row, and somehow WPF chokes on this.
        if (lvListing.ItemContainerGenerator.ContainerFromIndex(index) is ListViewItem item)
        {
            item.Focus();
        }
    }
#endif

    private void ChildHyperlink_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is Hyperlink hyperlink)
        {
            MessageBox.Show(string.Format(
                "Hyperlink clicked: Uri={0}", hyperlink.NavigateUri));
        }
    }
}
