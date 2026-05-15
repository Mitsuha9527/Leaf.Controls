using System.Windows;

namespace Leaf.Controls.CustomControls
{
    public class LeafNotificationBox
    {
        private static Window? GetOwnerWindow()
        {
            // 获取当前激活的窗口
            var activeWindow = Application
                .Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.IsActive);
            return activeWindow;
        }

        public static void Show(string message, string? title = null)
        {
            var messageBox = new LeafMessageBox();
            messageBox.ShowInfo(message, title, GetOwnerWindow());
        }

        public static bool? ShowYesNo(
            string message,
            string? title = null,
            string yesText = "",
            string noText = ""
        )
        {
            var messageBox = new LeafMessageBox();
            return messageBox.ShowYesNo(message, title, yesText, noText, GetOwnerWindow());
        }

        public static void ShowWarning(string message, string? title = null)
        {
            var messageBox = new LeafMessageBox();
            messageBox.ShowWarning(message, title, GetOwnerWindow());
        }

        public static void ShowError(string message, string? title = null)
        {
            var messageBox = new LeafMessageBox();
            messageBox.ShowError(message, title, GetOwnerWindow());
        }

        public static string ShowPasswordPrompt(string? title = null, Window? onwer = null)
        {
            var messageBox = new LeafMessageBox();
            return messageBox.ShowPasswordPrompt(title, GetOwnerWindow());
        }

        public static string ShowInputPrompt(string? title = null, Window? onwer = null)
        {
            var messageBox = new LeafMessageBox();
            return messageBox.ShowInputPrompt(title, GetOwnerWindow());
        }
    }
}
