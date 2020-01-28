using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace TilePanel
{
    public class AnimatingTilePanel : AnimatingPanel
    {
        #region public properties

        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public static double GetItemWidth(DependencyObject element)
        {
            Contract.Requires<ArgumentNullException>(element != null);
            return (double)element.GetValue(ItemWidthProperty);
        }

        public static void SetItemWidth(DependencyObject element, double itemWidth)
        {
            Contract.Requires<ArgumentNullException>(element != null);
            element.SetValue(ItemWidthProperty, itemWidth);
        }

        public static readonly DependencyProperty ItemWidthProperty =
            CreateDoubleDp("ItemWidth", 50, FrameworkPropertyMetadataOptions.AffectsMeasure, 0, double.PositiveInfinity, true);

        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public static double GetItemHeight(DependencyObject element)
        {
            Contract.Requires<ArgumentNullException>(element != null);
            return (double)element.GetValue(ItemHeightProperty);
        }

        public static void SetItemHeight(DependencyObject element, double itemHeight)
        {
            Contract.Requires<ArgumentNullException>(element != null);
            element.SetValue(ItemHeightProperty, itemHeight);
        }

        public static readonly DependencyProperty ItemHeightProperty =
            CreateDoubleDp("ItemHeight", 50, FrameworkPropertyMetadataOptions.AffectsMeasure, 0, double.PositiveInfinity, true);

        #endregion

        #region protected override

        protected override Size MeasureOverride(Size availableSize)
        {
            OnPreApplyTemplate();

            var theChildSize = GetItemSize();

            foreach (UIElement child in Children)
            {
                child.Measure(theChildSize);
            }

            // Figure out how many children fit on each row
            var childrenPerRow = double.IsPositiveInfinity(availableSize.Width) 
                ? Children.Count 
                : Math.Max(1, (int)Math.Floor(availableSize.Width / ItemWidth));

            // Calculate the width and height this results in
            var width = childrenPerRow * ItemWidth;
            var height = ItemHeight * (Math.Floor((double)Children.Count / childrenPerRow) + 1);
            height = (height.IsValid()) ? height : 0;
            return new Size(width, height);
        }

        protected sealed override Size ArrangeOverride(Size finalSize)
        {
            // Calculate how many children fit on each row
            var childrenPerRow = Math.Max(1, (int)Math.Floor(finalSize.Width / ItemWidth));
            var theChildSize = GetItemSize();

            for (var i = 0; i < Children.Count; i++)
            {
                // Figure out where the child goes
                var newOffset = CalculateChildOffset(i, childrenPerRow,
                    ItemWidth, ItemHeight,
                    finalSize.Width, Children.Count);

                ArrangeChild(Children[i], new Rect(newOffset, theChildSize));
            }

            ArrangedOnce = true;
            return finalSize;
        }

        protected override Point ProcessNewChild(UIElement child, Rect providedBounds)
        {
            var startLocation = providedBounds.Location;
            if (ArrangedOnce)
            {
                if (ItemOpacityAnimation == null)
                {
                    ItemOpacityAnimation = new DoubleAnimation()
                    {
                        From = 0,
                        Duration = new Duration(TimeSpan.FromSeconds(.5))
                    };
                    ItemOpacityAnimation.Freeze();
                }

                child.BeginAnimation(OpacityProperty, ItemOpacityAnimation);
                startLocation -= new Vector(providedBounds.Width, 0);
            }
            return startLocation;
        }

        #endregion

        #region Implementation

        #region private methods

        private Size GetItemSize() { return new Size(ItemWidth, ItemHeight); }

        private void BindToParentItemsControl(DependencyProperty property, DependencyObject source)
        {
            if (DependencyPropertyHelper.GetValueSource(this, property).BaseValueSource == BaseValueSource.Default)
            {
                var binding = new Binding {Source = source, Path = new PropertyPath(property)};
                SetBinding(property, binding);
            }
        }

        private void OnPreApplyTemplate()
        {
            if (!AppliedTemplate)
            {
                AppliedTemplate = true;

                var source = TemplatedParent;
                if (source is ItemsPresenter)
                {
                    source = source.FindParent<ItemsControl>();
                }

                if (source != null)
                {
                    BindToParentItemsControl(ItemHeightProperty, source);
                    BindToParentItemsControl(ItemWidthProperty, source);
                }
            }
        }

        // Given a child index, child size and children per row, figure out where the child goes
        private static Point CalculateChildOffset(
            int index,
            int childrenPerRow,
            double itemWidth,
            double itemHeight,
            double panelWidth,
            int totalChildren)
        {
            double fudge = 0;
            if (totalChildren > childrenPerRow)
            {
                fudge = (panelWidth - childrenPerRow * itemWidth) / childrenPerRow;
                Debug.Assert(fudge >= 0);
            }

            var row = index / childrenPerRow;
            var column = index % childrenPerRow;
            return new Point(.5 * fudge + column * (itemWidth + fudge), row * itemHeight);
        }

        #endregion

        protected bool AppliedTemplate { get; set; }
        protected bool ArrangedOnce { get; set; }
        protected DoubleAnimation ItemOpacityAnimation { get; set; }

        #endregion
    } 
}
