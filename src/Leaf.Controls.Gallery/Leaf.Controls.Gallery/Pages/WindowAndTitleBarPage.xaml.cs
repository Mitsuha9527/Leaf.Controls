using System.Windows;
using System.Windows.Controls;
using Leaf.Controls.Gallery.Windows;

namespace Leaf.Controls.Gallery.Pages;

public partial class WindowAndTitleBarPage : Page
{
    public WindowAndTitleBarPage()
    {
        InitializeComponent();
    }

    private void OnOpenLeafWindowClick(object sender, RoutedEventArgs e)
    {
        var owner = Window.GetWindow(this);
        var demoWindow = new LeafWindowDemoWindow
        {
            Owner = owner,
        };
        demoWindow.ShowDialog();
    }

    private void OnMinimizeMockClick(object sender, RoutedEventArgs e)
    {
        var owner = Window.GetWindow(this);
        if (owner is not null)
        {
            owner.WindowState = WindowState.Minimized;
        }
    }
}
