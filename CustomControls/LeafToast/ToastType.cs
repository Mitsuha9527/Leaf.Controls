using System;

namespace Leaf.Controls.CustomControls
{
    /// <summary>
    /// Toast 消息类型
    /// </summary>
    public enum ToastType
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success,

        /// <summary>
        /// 信息
        /// </summary>
        Info,

        /// <summary>
        /// 警告
        /// </summary>
        Warning,

        /// <summary>
        /// 错误
        /// </summary>
        Error,

        /// <summary>
        /// 致命错误
        /// </summary>
        Fatal,

        /// <summary>
        /// 询问
        /// </summary>
        Ask
    }
}
