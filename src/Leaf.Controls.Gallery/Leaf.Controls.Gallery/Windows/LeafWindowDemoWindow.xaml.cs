using System.Windows;

namespace Leaf.Controls.Gallery.Windows;

public partial class LeafWindowDemoWindow
{
    public LeafWindowDemoWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
