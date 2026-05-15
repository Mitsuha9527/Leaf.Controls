using System.Windows;
using System.Windows.Controls;

namespace Leaf.Controls.Gallery;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly List<NavigationItem> _items;

    public MainWindow()
    {
        InitializeComponent();

        _items =
        [
            new NavigationItem("概览", () => new Pages.OverviewPage()),
            new NavigationItem("窗口与标题栏", () => new Pages.WindowAndTitleBarPage()),
            new NavigationItem("输入控件", () => new Pages.InputControlsPage()),
            new NavigationItem("按钮与显示", () => new Pages.DisplayControlsPage()),
            new NavigationItem("容器与导航", () => new Pages.ContainerControlsPage()),
            new NavigationItem("数据与反馈", () => new Pages.FeedbackControlsPage()),
            new NavigationItem("图像控件", () => new Pages.ImageControlsPage()),
            new NavigationItem("附加属性", () => new Pages.AttachPropertiesPage()),
        ];

        NavigationList.ItemsSource = _items;
        NavigationList.SelectedIndex = 0;
    }

    private void NavigationList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavigationList.SelectedItem is not NavigationItem item)
        {
            return;
        }

        ContentFrame.Navigate(item.PageFactory());
        PageTitleText.Text = item.Title;
    }
}

internal sealed class NavigationItem
{
    public NavigationItem(string title, Func<Page> pageFactory)
    {
        Title = title;
        PageFactory = pageFactory;
    }

    public string Title { get; }

    public Func<Page> PageFactory { get; }
}