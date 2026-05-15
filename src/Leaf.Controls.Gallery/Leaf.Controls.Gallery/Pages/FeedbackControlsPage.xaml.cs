using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Leaf.Controls.CustomControls;

namespace Leaf.Controls.Gallery.Pages;

public partial class FeedbackControlsPage : Page
{
    public ObservableCollection<DemoRow> DemoRows { get; } =
    [
        new DemoRow("相机连接", "正常", "Online"),
        new DemoRow("曝光参数", "正常", "8.3 ms"),
        new DemoRow("工单号", "警告", "JOB-00017"),
        new DemoRow("产线节拍", "正常", "1.28 s"),
    ];

    public FeedbackControlsPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void OnToastSuccessClick(object sender, RoutedEventArgs e)
    {
        LeafToast.Success("操作已完成。");
    }

    private void OnToastWarningClick(object sender, RoutedEventArgs e)
    {
        LeafToast.Warning("当前参数接近上限，请检查。");
    }

    private void OnToastAskClick(object sender, RoutedEventArgs e)
    {
        LeafToast.Ask(
            "是否将当前结果标记为通过？",
            () => LeafToast.Success("你已选择通过。"),
            () => LeafToast.Info("你已取消本次操作。")
        );
    }

    private void OnNotificationClick(object sender, RoutedEventArgs e)
    {
        LeafNotificationBox.Show("这是 LeafNotificationBox 示例。");
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        bool? result = LeafNotificationBox.ShowYesNo("是否应用当前配置？", "确认");
        if (result == true)
        {
            LeafToast.Success("配置已应用。");
        }
        else if (result == false)
        {
            LeafToast.Warning("你拒绝了配置应用。");
        }
    }

    private void OnInputPromptClick(object sender, RoutedEventArgs e)
    {
        string value = LeafNotificationBox.ShowInputPrompt("请输入工单号");
        if (string.IsNullOrWhiteSpace(value))
        {
            LeafToast.Info("未输入内容。");
            return;
        }

        LeafToast.Info($"已输入：{value}");
    }
}

public sealed record DemoRow(string Name, string Status, string Value);
