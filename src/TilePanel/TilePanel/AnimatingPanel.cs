using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TilePanel
{
    public abstract class AnimatingPanel : Panel
    {
        protected AnimatingPanel()
        {
            MListener.Rendering += compositionTarget_Rendering;
            MListener.WireParentLoadedUnloaded(this);
        }


        private static readonly DependencyProperty DataProperty = DependencyProperty.RegisterAttached("Data", typeof(AnimatingPanelItemData), typeof(AnimatingTilePanel));
        private CompositionTargetRenderingListener MListener { get; } = new CompositionTargetRenderingListener();
        private const double CDiff = 0.1;
        private const double CTerminalVelocity = 10000;

        #region DPs

        public static readonly DependencyProperty AttractionProperty = CreateDoubleDp(nameof(Attraction), 2, FrameworkPropertyMetadataOptions.None, 0, double.PositiveInfinity, false);
        public static readonly DependencyProperty VariationProperty = CreateDoubleDp(nameof(Variation), 1, FrameworkPropertyMetadataOptions.None, 0, true, 1, true, false);
        public static readonly DependencyProperty DampeningProperty = CreateDoubleDp(nameof(Dampening), 0.2, FrameworkPropertyMetadataOptions.None, 0, 1, false);
        
        public double Dampening
        {
            get => (double)GetValue(DampeningProperty);
            set => SetValue(DampeningProperty, value);
        }
        
        public double Attraction
        {
            get => (double)GetValue(AttractionProperty);
            set => SetValue(AttractionProperty, value);
        }
        
        public double Variation
        {
            get => (double)GetValue(VariationProperty);
            set => SetValue(VariationProperty, value);
        }
        
        #endregion

        protected virtual Point ProcessNewChild(UIElement child, Rect providedBounds)
        {
            return providedBounds.Location;
        }

        protected void ArrangeChild(UIElement child, Rect bounds)
        {
            MListener.StartListening();

            var data = (AnimatingPanelItemData)child.GetValue(DataProperty);
            if (data == null)
            {
                data = new AnimatingPanelItemData();
                child.SetValue(DataProperty, data);
                Debug.Assert(child.RenderTransform == Transform.Identity);
                child.RenderTransform = data.Transform;

                data.Current = ProcessNewChild(child, bounds);
            }
            Debug.Assert(child.RenderTransform == data.Transform);

            //set the location attached DP
            data.Target = bounds.Location;

            child.Arrange(bounds);
        }

        private void compositionTarget_Rendering(object sender, EventArgs e)
        {
            var dampening = Dampening;
            var attractionFactor = Attraction * .01;
            var variation = Variation;

            var shouldChange = false;
            for (var i = 0; i < Children.Count; i++)
            {
                shouldChange = UpdateChildData(
                    (AnimatingPanelItemData)Children[i].GetValue(DataProperty),
                    dampening,
                    attractionFactor,
                    variation) || shouldChange;
            }

            if (!shouldChange)
            {
                MListener.StopListening();
            }
        }

        private static bool UpdateChildData(AnimatingPanelItemData data, double dampening, double attractionFactor, double variation)
        {
            if (data == null)
                return false;

            Debug.Assert(dampening > 0 && dampening < 1);
            Debug.Assert(attractionFactor > 0 && !double.IsInfinity(attractionFactor));

            attractionFactor *= 1 + (variation * data.RandomSeed - 0.5);

            var anythingChanged =
                GeoHelper.Animate(data.Current, data.LocationVelocity, data.Target,
                    attractionFactor, dampening, CTerminalVelocity, CDiff, CDiff,
                    out var newLocation, out var newVelocity);

            data.Current = newLocation;
            data.LocationVelocity = newVelocity;

            var transformVector = data.Current - data.Target;
            data.Transform.SetToVector(transformVector);

            return anythingChanged;
        }


        protected static DependencyProperty CreateDoubleDp(
          string name,
          double defaultValue,
          FrameworkPropertyMetadataOptions metadataOptions,
          double minValue,
          double maxValue,
          bool attached)
        {
            return CreateDoubleDp(name, defaultValue, metadataOptions, minValue, false, maxValue, false, attached);
        }

        protected static DependencyProperty CreateDoubleDp(
            string name,
            double defaultValue,
            FrameworkPropertyMetadataOptions metadataOptions,
            double minValue,
            bool includeMin,
            double maxValue,
            bool includeMax,
            bool attached)
        {
            Contract.Requires(!double.IsNaN(minValue));
            Contract.Requires(!double.IsNaN(maxValue));
            Contract.Requires(maxValue >= minValue);

            ValidateValueCallback validateValueCallback = delegate (object objValue)
            {
                var value = (double)objValue;

                if (includeMin)
                {
                    if (value < minValue) return false;
                }
                else
                {
                    if (value <= minValue) return false;
                }
                if (includeMax)
                {
                    if (value > maxValue) return false;
                }
                else
                {
                    if (value >= maxValue) return false;
                }

                return true;
            };

            if (attached)
            {
                return DependencyProperty.RegisterAttached(
                    name,
                    typeof(double),
                    typeof(AnimatingTilePanel),
                    new FrameworkPropertyMetadata(defaultValue, metadataOptions), validateValueCallback);
            }
            else
            {
                return DependencyProperty.Register(
                    name,
                    typeof(double),
                    typeof(AnimatingTilePanel),
                    new FrameworkPropertyMetadata(defaultValue, metadataOptions), validateValueCallback);
            }
        }
        

        private class AnimatingPanelItemData
        {
            public Point Target;
            public Point Current;
            public Vector LocationVelocity;
            public readonly double RandomSeed = Util.Rnd.NextDouble();
            public readonly TranslateTransform Transform = new TranslateTransform();
        }
    }
}
