using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Leaf.Controls.CustomControls
{
    [TemplatePart(Name = PART_VerLine, Type = typeof(Rectangle))]
    [TemplatePart(Name = PART_MainBorder, Type = typeof(Border))]
    public class TreeViewItem : System.Windows.Controls.TreeViewItem
    {
        const string PART_VerLine = "PART_VerLine";
        const string PART_MainBorder = "PART_MainBorder";
        private Rectangle? _verLine;
        private Border? _mainBorder;
        public Rectangle? VerLine => _verLine;
        public Action? LastItemAdd { get; set; }

        static TreeViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TreeViewItem),
                new FrameworkPropertyMetadata(typeof(TreeViewItem))
            );
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _verLine = GetTemplateChild(PART_VerLine) as Rectangle;
            _mainBorder = GetTemplateChild(PART_MainBorder) as Border;
            LastItemAdd?.Invoke();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Border border && border.Name == PART_MainBorder)
            {
                e.Handled = true;
                return;
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            // 直接检查事件源是否为当前 TreeViewItem 实例
            DependencyObject? originalSource = e.OriginalSource as DependencyObject;
            if (originalSource == null)
            {
                base.OnMouseDoubleClick(e);
                return;
            }

            // 使用更高效的判断逻辑：先检查源是否为 TreeViewItem 类型
            if (originalSource is TreeViewItem sourceItem)
            {
                // 如果是子 TreeViewItem，阻止冒泡
                if (sourceItem != this)
                    return;
            }
            else
            {
                // 查找实际的事件源
                bool isChildTreeViewItem = false;
                DependencyObject current = originalSource;

                bool isFirstParent = false;
                while (current != null && current != this)
                {
                    // 使用模式匹配简化类型检查
                    if (current is TreeViewItem && current != this)
                    {
                        isChildTreeViewItem = true;
                        break;
                    }
                    current = VisualTreeHelper.GetParent(current);
                    // 只有点击ContentPresenter才可以触发
                    if (!isFirstParent)
                    {
                        isFirstParent = true;
                        var firstParent = current as ContentPresenter;
                        if (firstParent is null)
                            return;
                    }
                }

                // 如果是子 TreeViewItem 或没有到达当前控件，则退出
                if (isChildTreeViewItem || current != this)
                    return;
            }

            if (ItemDoubleClickCommand is not null)
            {
                var parameter = ItemDoubleClickCommandParameter ?? DataContext;
                ItemDoubleClickCommand.Execute(parameter);
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItem();
        }

        // 这个方法在容器创建后进行配置
        protected override void PrepareContainerForItemOverride(
            DependencyObject element,
            object item
        )
        {
            base.PrepareContainerForItemOverride(element, item);

            // 获取当前TreeViewItem
            var treeViewItem = element as TreeViewItem;
            if (treeViewItem != null)
            {
                // 判断是否为最后一项
                int index = ItemContainerGenerator.IndexFromContainer(treeViewItem);
                if (index >= 0)
                {
                    treeViewItem.IsLastItem = (index == Items.Count - 1);
                    if (treeViewItem.IsLastItem)
                    {
                        treeViewItem.LastItemAdd = () =>
                        {
                            treeViewItem.VerLine.VerticalAlignment = VerticalAlignment.Top;
                            treeViewItem.VerLine.Height = 10;
                        };
                    }

                    if (index >= 1)
                    {
                        var tempItem =
                            ItemContainerGenerator.ContainerFromIndex(index - 1) as TreeViewItem;
                        if (tempItem?.VerLine is not null)
                        {
                            tempItem.VerLine.VerticalAlignment = VerticalAlignment.Stretch;
                            tempItem.VerLine.Height = double.NaN;
                        }
                    }
                }
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    // 获取数据项
                    var dataItem = Items[i];

                    // 使用ItemContainerGenerator获取对应的容器控件
                    var container = ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;

                    if (container is not null)
                    {
                        if (i == Items.Count - 1)
                        {
                            container.VerLine.VerticalAlignment = VerticalAlignment.Top;
                            container.VerLine.Height = 10;
                        }
                        else
                        {
                            container.VerLine.VerticalAlignment = VerticalAlignment.Stretch;
                            container.VerLine.Height = double.NaN;
                        }
                    }
                }
            }
        }

        #region Dependency Properties

        public static readonly DependencyProperty IsLastItemProperty = DependencyProperty.Register(
            nameof(IsLastItem),
            typeof(bool),
            typeof(TreeViewItem),
            new PropertyMetadata(false)
        );
        public bool IsLastItem
        {
            get { return (bool)GetValue(IsLastItemProperty); }
            set { SetValue(IsLastItemProperty, value); }
        }

        public static readonly DependencyProperty CheckedEnableProperty =
            DependencyProperty.Register(
                nameof(CheckedEnable),
                typeof(bool),
                typeof(TreeViewItem),
                new PropertyMetadata(false)
            );
        public bool CheckedEnable
        {
            get { return (bool)GetValue(CheckedEnableProperty); }
            set { SetValue(CheckedEnableProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            nameof(IsChecked),
            typeof(bool),
            typeof(TreeViewItem),
            new PropertyMetadata(false, OnItemCheckedChanged)
        );

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        protected virtual void OnIsCheckedChanged(bool oldValue, bool newValue)
        {
            if (!CheckedEnable)
                return;

            if (
                CheckedChangedCommand != null
                && CheckedChangedCommand.CanExecute(CheckedChangedCommandParameter ?? newValue)
            )
            {
                CheckedChangedCommand.Execute(CheckedChangedCommandParameter ?? newValue);
            }
            RoutedPropertyChangedEventArgs<bool> args = new RoutedPropertyChangedEventArgs<bool>(
                oldValue,
                newValue,
                ItemCheckedChangedEvent
            );
            RaiseEvent(args);
        }

        private static void OnItemCheckedChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            TreeViewItem treeViewItem = (TreeViewItem)d;
            if (treeViewItem != null)
            {
                bool oldValue = (bool)e.OldValue;
                bool newValue = (bool)e.NewValue;
                treeViewItem.OnIsCheckedChanged(oldValue, newValue);
            }
        }

        public static readonly DependencyProperty ItemDoubleClickCommandProperty =
            DependencyProperty.Register(
                nameof(ItemDoubleClickCommand),
                typeof(ICommand),
                typeof(TreeViewItem),
                new PropertyMetadata(null)
            );
        public ICommand ItemDoubleClickCommand
        {
            get { return (ICommand)GetValue(ItemDoubleClickCommandProperty); }
            set { SetValue(ItemDoubleClickCommandProperty, value); }
        }

        public static readonly DependencyProperty ItemDoubleClickCommandParameterProperty =
            DependencyProperty.Register(
                nameof(ItemDoubleClickCommandParameter),
                typeof(object),
                typeof(TreeViewItem),
                new PropertyMetadata(null)
            );
        public object ItemDoubleClickCommandParameter
        {
            get { return GetValue(ItemDoubleClickCommandParameterProperty); }
            set { SetValue(ItemDoubleClickCommandParameterProperty, value); }
        }

        public static readonly DependencyProperty CheckedChangedCommandProperty =
            DependencyProperty.Register(
                nameof(CheckedChangedCommand),
                typeof(ICommand),
                typeof(TreeViewItem),
                new PropertyMetadata(null)
            );

        public ICommand CheckedChangedCommand
        {
            get { return (ICommand)GetValue(CheckedChangedCommandProperty); }
            set { SetValue(CheckedChangedCommandProperty, value); }
        }

        public static readonly DependencyProperty CheckedChangedCommandParameterProperty =
            DependencyProperty.Register(
                nameof(CheckedChangedCommandParameter),
                typeof(object),
                typeof(TreeViewItem),
                new PropertyMetadata(null)
            );

        public object CheckedChangedCommandParameter
        {
            get { return GetValue(CheckedChangedCommandParameterProperty); }
            set { SetValue(CheckedChangedCommandParameterProperty, value); }
        }

        public static readonly RoutedEvent ItemCheckedChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(ItemCheckedChanged),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(TreeViewItem)
            );

        public event RoutedEventHandler ItemCheckedChanged
        {
            add { AddHandler(ItemCheckedChangedEvent, value); }
            remove { RemoveHandler(ItemCheckedChangedEvent, value); }
        }
        #endregion
    }
}
