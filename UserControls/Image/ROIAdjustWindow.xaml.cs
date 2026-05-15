using Leaf.Controls.CustomControls;

namespace Leaf.Controls.UserControls.Image
{
    /// <summary>
    /// ROIAdjustWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ROIAdjustWindow : LeafWindow
    {
        public ROIAdjustWindow()
        {
            InitializeComponent();
        }

        public required Action<double> MoveUp { get; set; }
        public required Action<double> MoveLeft { get; set; }
        public required Action<double> MoveRight { get; set; }
        public required Action<double> MoveDown { get; set; }

        private void MoveUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MoveUp?.Invoke(AdjustValue_Numberbox.Value);
        }

        private void MoveLeft_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MoveLeft?.Invoke(AdjustValue_Numberbox.Value);
        }

        private void MoveRight_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MoveRight?.Invoke(AdjustValue_Numberbox.Value);
        }

        private void MoveDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MoveDown?.Invoke(AdjustValue_Numberbox.Value);
        }
    }
}
