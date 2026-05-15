# Leaf.Controls

一个面向 .NET 8 的现代化 WPF 控件库，旨在通过提供一系列自定义控件、附加属性和实用工具来简化 UI 开发。

## 功能特性

Leaf.Controls 提供了多种 UI 组件和辅助类：

### 控件 (Controls)
- **LeafWindow**: 支持现代样式的自定义窗口。
- **LeafMessageBox**: 增强型消息框，提供更好的用户交互。
- **LeafToast**: 非侵入式的 Toast 通知。
- **TitleBar**: 配合 `TitleBarButton` 使用的可自定义标题栏。
- **NumberBox**: 针对数值输入优化的输入控件。
- **DateTimePicker**: 多功能的日期和时间选择器。
- **ToggleLabel**: 可切换的标签控件。
- **SimplePanel**: 轻量级的面板容器。
- **ImageViewer / WrapImageViewer**: 用于显示和管理图像的控件，支持 ROI 形状。

### 附加属性 (Attach Properties)
- **Attach**: 针对 `DataGrid`、`TextBox`、`CheckBox`、`Border` (`BorderElement`) 和 `IconElement` 的扩展，增加了额外的功能和样式能力。

### 实用工具 (Utilities)
- **ThemeManager**: 管理应用程序主题的帮助类。
- **Converters**: 全面的值转换器集合（例如 `BooleanToVisibilityConverter`, `EnumDescriptionConverter`, `ThicknessSplitConverter`）。
- **Commands**: 用于 MVVM 模式的 `RelayCommand` 实现。

## 安装

您可以在项目中引用 `Leaf.Controls` 库。

(如果已发布到 NuGet)
```xml
<PackageReference Include="Leaf.Controls" Version="1.0.2" />
```

## 使用方法

1. 修改 `App.xaml`，引入资源字典：
```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Leaf.Controls;component/Themes/ThemesColors/ColorLight.xaml" />
            <ResourceDictionary Source="pack://application:,,,/Leaf.Controls;component/Themes/Leaf.Ui.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

2. 在 XAML 中引入命名空间：
```xml
xmlns:leaf="http://schemas.Leaf.Controls.com"
```

3. 使用控件示例：
```xml
<leaf:LeafWindow x:Class="MyApp.MainWindow"
                 xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                 xmlns:leaf="http://schemas.Leaf.Controls.com"
                 Title="My Application">
    <Grid>
        <leaf:NumberBox Value="100" />
    </Grid>
</leaf:LeafWindow>
```

## 许可证 (License)

本项目采用 MIT 许可证 - 详情请参阅 [LICENSE](LICENSE) 文件。
