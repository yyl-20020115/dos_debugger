using System.Windows.Controls;

namespace WpfDebugger;

/// <summary>
/// Interaction logic for PropertiesControl.xaml
/// </summary>
public partial class PropertiesControl : UserControl
{
    public PropertiesControl()
    {
        InitializeComponent();
    }

#if false
    public object SelectedObject
    {
        get { return propertyGrid.SelectedObject; }
        set { propertyGrid.SelectedObject = value; }
    }
#else
    public object SelectedObject { get; set; }
#endif

    private int[] testArray;
    public int[] TestArray
    {
        get
        {
            if (testArray == null)
            {
                testArray = new int[100];
                for (int i = 0; i < testArray.Length; i++)
                {
                    testArray[i] = i * 2 + 1;
                }
            }
            return testArray;
        }
    }
}
