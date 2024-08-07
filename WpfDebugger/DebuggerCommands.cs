using System.Windows.Input;

namespace WpfDebugger;

static class DebuggerCommands
{
    public static readonly RoutedCommand OpenDisassembly = new();
    public static readonly RoutedCommand OpenNewDisassembly = new();
    public static readonly RoutedCommand OpenHexView = new();
    public static readonly RoutedCommand OpenNewHexView = new();
}
