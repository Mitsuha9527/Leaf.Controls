using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Leaf.Controls.Properties;

namespace Leaf.Controls.CustomControls
{
    /// <summary>
    /// LeafMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class LeafMessageBox : LeafWindow
    {
        private bool? _messageDialogResult = null;

        private DispatcherTimer _keyFocusTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500),
        };

        public LeafMessageBox()
        {
            InitializeComponent();
            ButtonYes.Content = Lang.Button_Yes;
            ButtonNo.Content = Lang.Button_No;
            ButtonCancel.Content = Lang.Button_Cancel;
            ButtonConfirm.Content = Lang.Button_Confirm;
            _keyFocusTimer.Tick += (s, e) =>
            {
                if (Grid_Password.Visibility == Visibility.Visible)
                {
                    PasswordBox_Input.Focus();
                }
                else if (TextBox_Input.Visibility == Visibility.Visible)
                {
                    TextBox_Input.Focus();
                }
            };
            _keyFocusTimer.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _keyFocusTimer.Stop();
            base.OnClosing(e);
        }

        private void Show(string message, string? title = null, Window? onwer = null)
        {
            this.Owner = onwer;

            Text_Title.Text = title ?? "Infomation";
            Text_Message.Text = message;
            ButtonYes.Visibility = Visibility.Collapsed;
            ButtonNo.Visibility = Visibility.Collapsed;
            this.ShowDialog();
        }

        public void ShowInfo(string message, string? title = null, Window? onwer = null)
        {
            var icon = Resources["InfomationIcon"] as DrawingImage;
            IconElement.SetDrawingIcon(TitleBar, icon!);
            Show(message, title, onwer);
        }

        public bool? ShowYesNo(
            string message,
            string? title = null,
            string yesText = "",
            string noText = "",
            Window? onwer = null
        )
        {
            this.Owner = onwer;
            var icon = Resources["InfomationIcon"] as DrawingImage;
            IconElement.SetDrawingIcon(TitleBar, icon!);
            Text_Title.Text = title ?? "Infomation";
            Text_Message.Text = message;
            ButtonConfirm.Visibility = Visibility.Collapsed;
            ButtonCancel.Visibility = Visibility.Visible;
            if (!string.IsNullOrEmpty(yesText))
                ButtonYes.Content = yesText;
            if (!string.IsNullOrEmpty(noText))
                ButtonNo.Content = noText;
            this.ShowDialog();
            return _messageDialogResult;
        }

        public void ShowWarning(string message, string? title = null, Window? onwer = null)
        {
            var icon = Resources["WarningIcon"] as DrawingImage;
            IconElement.SetDrawingIcon(TitleBar, icon!);
            title ??= "Warning";
            Show(message, title, onwer);
        }

        public void ShowError(string message, string? title = null, Window? onwer = null)
        {
            var icon = Resources["ErrorIcon"] as DrawingImage;
            IconElement.SetDrawingIcon(TitleBar, icon!);
            title ??= "Error";
            Show(message, title, onwer);
        }

        public string ShowPasswordPrompt(string? title = null, Window? onwer = null)
        {
            var icon = Resources["InfomationIcon"] as DrawingImage;
            IconElement.SetDrawingIcon(TitleBar, icon!);
            Text_Title.Text = title ?? "Password Required";
            this.Owner = onwer;
            Grid_Password.Visibility = Visibility.Visible;
            TextBox_Input.Visibility = Visibility.Collapsed;
            Text_Message.Visibility = Visibility.Collapsed;
            ButtonYes.Visibility = Visibility.Collapsed;
            ButtonNo.Visibility = Visibility.Collapsed;
            this.ShowDialog();
            return PasswordBox_Input.Password;
        }

        public string ShowInputPrompt(string? title = null, Window? onwer = null)
        {
            var icon = Resources["InfomationIcon"] as DrawingImage;
            IconElement.SetDrawingIcon(TitleBar, icon!);
            Text_Title.Text = title ?? "Input Required";
            this.Owner = onwer;
            Grid_Password.Visibility = Visibility.Collapsed;
            TextBox_Input.Visibility = Visibility.Visible;
            Text_Message.Visibility = Visibility.Collapsed;
            ButtonYes.Visibility = Visibility.Collapsed;
            ButtonNo.Visibility = Visibility.Collapsed;
            this.ShowDialog();
            return TextBox_Input.Text;
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            _messageDialogResult = true;
            this.Close();
        }

        private void ButtonNo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            _messageDialogResult = false;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = null;
            _messageDialogResult = null;
            this.Close();
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            _messageDialogResult = true;
            this.Close();
        }

        private void PasswordBox_Input_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                ButtonConfirm_Click(sender, e);
            }
        }

        private void TextBox_Input_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                ButtonConfirm_Click(sender, e);
            }
        }
    }
}
