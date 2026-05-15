using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Leaf.Controls.CustomControls
{
    public class AdornerContainer : Adorner
    {
        private UIElement? child;

        
        public UIElement? Child
        {
            get
            {
                return child;
            }
            set
            {
                AddVisualChild(value);
                child = value;
            }
        }

        protected override int VisualChildrenCount => (child != null) ? 1 : 0;

        public AdornerContainer(UIElement adornedElement)
            : base(adornedElement)
        {
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (child != null)
            {
                child.Arrange(new Rect(finalSize));
            }

            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0 || child == null)
            {
                return base.GetVisualChild(index);
            }

            return child;
        }
    }
}
