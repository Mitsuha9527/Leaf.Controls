using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

namespace Leaf.Controls.Utilities
{
    /// <summary>
    /// 主题管理器，用于切换应用程序主题
    /// </summary>
    public static class ThemeManager
    {
        private const string DarkThemeUri =
            "pack://application:,,,/Leaf.Controls;component/Themes/ThemesColors/ColorDark.xaml";
        private const string LightThemeUri =
            "pack://application:,,,/Leaf.Controls;component/Themes/ThemesColors/ColorLight.xaml";
        private const string VioletThemeUri =
            "pack://application:,,,/Leaf.Controls;component/Themes/ThemesColors/ColorViolet.xaml";

        private static Theme _currentTheme = Theme.Light;
        private static bool _isInitialized = false;

        /// <summary>
        /// 当前主题
        /// </summary>
        public static Theme CurrentTheme
        {
            get => _currentTheme;
            private set
            {
                if (_currentTheme != value)
                {
                    var oldTheme = _currentTheme;
                    _currentTheme = value;
                    ThemeChanged?.Invoke(oldTheme, value);
                }
            }
        }

        /// <summary>
        /// 主题切换事件
        /// </summary>
        public static event Action<Theme, Theme>? ThemeChanged;

        /// <summary>
        /// 应用指定主题
        /// </summary>
        /// <param name="theme">要应用的主题</param>
        public static void ApplyTheme(Theme theme)
        {
            var app = Application.Current;
            if (app?.Resources == null)
            {
                return;
            }
            var themeUri = theme switch
            {
                Theme.Light => LightThemeUri,
                Theme.Dark => DarkThemeUri,
                Theme.Violet => VioletThemeUri,
                _ => DarkThemeUri,
            };
            // 移除现有的主题资源字典
            RemoveThemeResourceDictionary(app.Resources);

            // 添加新的主题资源字典
            var themeResourceDict = new ResourceDictionary
            {
                Source = new Uri(themeUri, UriKind.Absolute),
            };
            // 将主题资源字典插入到最前面，确保优先级最高
            app.Resources.MergedDictionaries.Insert(0, themeResourceDict);
            CurrentTheme = theme;
        }

        /// <summary>
        /// 初始化主题管理器，应用默认主题
        /// </summary>
        /// <param name="defaultTheme">默认主题，如果不指定则使用浅色主题</param>
        public static void Initialize(Theme defaultTheme = Theme.Light)
        {
            if (_isInitialized)
            {
                return;
            }
            _isInitialized = true;
            CurrentTheme = defaultTheme;
            ApplyTheme(defaultTheme);
        }

        /// <summary>
        /// 移除现有的主题资源字典
        /// </summary>
        /// <param name="resources">应用程序资源</param>
        private static void RemoveThemeResourceDictionary(ResourceDictionary resources)
        {
            var toRemove = new List<ResourceDictionary>();
            foreach (var dict in resources.MergedDictionaries)
            {
                if (
                    dict.Source != null
                    && (
                        dict.Source.ToString().Contains("ThemesColors/ColorDark.xaml")
                        || dict.Source.ToString().Contains("ThemesColors/ColorLight.xaml")
                        || dict.Source.ToString().Contains("ThemesColors/ColorViolet.xaml")
                    )
                )
                {
                    toRemove.Add(dict);
                }
            }
            foreach (var dict in toRemove)
            {
                resources.MergedDictionaries.Remove(dict);
            }
        }

        /// <summary>
        /// 检查指定主题是否为当前主题
        /// </summary>
        /// <param name="theme">要检查的主题</param>
        /// <returns>如果是当前主题返回 true，否则返回 false</returns>
        public static bool IsCurrentTheme(Theme theme)
        {
            return CurrentTheme == theme;
        }
    }

    /// <summary>
    /// 主题枚举
    /// </summary>
    public enum Theme
    {
        /// <summary>
        /// 深色主题
        /// </summary>
        Dark,

        /// <summary>
        /// 浅色主题
        /// </summary>
        Light,

        /// <summary>
        /// 紫色主题
        /// </summary>
        Violet,
    }
}
