using System;
using System.Windows.Input;

namespace Leaf.Controls.CustomControls
{
    /// <summary>
    /// Toast 信息配置类
    /// </summary>
    public class LeafToastInfo
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 消息类型
        /// </summary>
        public ToastType Type { get; set; } = ToastType.Info;

        /// <summary>
        /// 是否显示关闭按钮
        /// </summary>
        public bool ShowCloseButton { get; set; } = true;

        /// <summary>
        /// 是否可以通过点击消息关闭
        /// </summary>
        public bool ClickToClose { get; set; } = false;

        /// <summary>
        /// 自动关闭时间（毫秒），0 表示不自动关闭
        /// </summary>
        public int WaitTime { get; set; } = 2000;

        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 图标画刷
        /// </summary>
        public string IconBrushKey { get; set; } = string.Empty;        /// <summary>
        /// 确认回调
        /// </summary>
        public Action? ConfirmAction { get; set; }

        /// <summary>
        /// 取消回调
        /// </summary>
        public Action? CancelAction { get; set; }

        /// <summary>
        /// 确认命令
        /// </summary>
        public ICommand? ConfirmCommand { get; set; }

        /// <summary>
        /// 取消命令
        /// </summary>
        public ICommand? CancelCommand { get; set; }

        /// <summary>
        /// 确认按钮文本
        /// </summary>
        public string ConfirmStr { get; set; } = "确认";

        /// <summary>
        /// 取消按钮文本
        /// </summary>
        public string CancelStr { get; set; } = "取消";

        /// <summary>
        /// 是否显示确认和取消按钮
        /// </summary>
        public bool ShowButtons => Type == ToastType.Ask;

        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static LeafToastInfo CreateDefault() => new LeafToastInfo();

        /// <summary>
        /// 创建成功消息
        /// </summary>
        public static LeafToastInfo CreateSuccess(string message) => new LeafToastInfo
        {
            Message = message,
            Type = ToastType.Success,
            Icon = "CheckCircleOutline",
            IconBrushKey = "SuccessBrush"
        };

        /// <summary>
        /// 创建信息消息
        /// </summary>
        public static LeafToastInfo CreateInfo(string message) => new LeafToastInfo
        {
            Message = message,
            Type = ToastType.Info,
            Icon = "InformationOutline",
            IconBrushKey = "InfoBrush"
        };

        /// <summary>
        /// 创建警告消息
        /// </summary>
        public static LeafToastInfo CreateWarning(string message) => new LeafToastInfo
        {
            Message = message,
            Type = ToastType.Warning,
            Icon = "AlertOutline",
            IconBrushKey = "WarningBrush"
        };

        /// <summary>
        /// 创建错误消息
        /// </summary>
        public static LeafToastInfo CreateError(string message) => new LeafToastInfo
        {
            Message = message,
            Type = ToastType.Error,
            Icon = "CloseCircleOutline",
            IconBrushKey = "ErrorBrush"
        };

        /// <summary>
        /// 创建致命错误消息
        /// </summary>
        public static LeafToastInfo CreateFatal(string message) => new LeafToastInfo
        {
            Message = message,
            Type = ToastType.Fatal,
            Icon = "AlertCircleOutline",
            IconBrushKey = "FatalBrush",
            WaitTime = 0 // 致命错误不自动关闭
        };        /// <summary>
        /// 创建询问消息
        /// </summary>
        public static LeafToastInfo CreateAsk(string message, Action? confirmAction = null, Action? cancelAction = null) => new LeafToastInfo
        {
            Message = message,
            Type = ToastType.Ask,
            Icon = "HelpCircleOutline",
            IconBrushKey = "AskBrush",
            ConfirmAction = confirmAction,
            CancelAction = cancelAction,
            WaitTime = 0 // 询问消息不自动关闭
        };
    }
}
