using System.Windows;
using System.Windows.Markup;

namespace Leaf.Controls.Utilities
{
    [Localizability(LocalizationCategory.Ignore)]
    [Ambient]
    [UsableDuringInitialization(true)]
    public class ControlsDictionary : ResourceDictionary
    {
        private const string DictionaryUri = "pack://application:,,,/Leaf.Controls;component/Themes/Leaf.Ui.xaml";

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlsDictionary"/> class.
        /// Default constructor defining <see cref="ResourceDictionary.Source"/> of the <c>WPF UI</c> controls dictionary.
        /// </summary>
        public ControlsDictionary()
        {
            Source = new Uri(DictionaryUri, UriKind.Absolute);
        }

        /// <summary>
        /// Initializes the controls dictionary with theme support.
        /// </summary>
        /// <param name="defaultTheme">The default theme to apply</param>
        public ControlsDictionary(Theme defaultTheme) : this()
        {
            // Initialize theme manager with the specified default theme
            if (!DesignerHelper.IsInDesignMode)
            {
                ThemeManager.Initialize(defaultTheme);
            }
        }
    }
}
