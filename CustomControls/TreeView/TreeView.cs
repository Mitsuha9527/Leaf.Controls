using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Leaf.Controls.CustomControls
{
    public class TreeView : System.Windows.Controls.TreeView
    {
        static TreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TreeView),
                new FrameworkPropertyMetadata(typeof(TreeView))
            );
        }

        // 这个方法告诉TreeView使用我们的自定义TreeViewItem作为容器
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
    }
}
