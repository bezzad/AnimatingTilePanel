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
            _mListener.Rendering += compositionTarget_Rendering;
            _mListener.WireParentLoadedUnloaded(this);
        }

        #region DPs

        public double Dampening
        {
            get => (double)GetValue(DampeningProperty);
            set => SetValue(DampeningProperty, value);
        }

        public static readonly DependencyProperty DampeningProperty =
            CreateDoubleDp("Dampening", 0.2, FrameworkPropertyMetadataOptions.None, 0, 1, false);

        public double Attraction
        {
            get => (double)GetValue(AttractionProperty);
            set => SetValue(AttractionProperty, value);
        }

        public static readonly DependencyProperty AttractionProperty =
            CreateDoubleDp("Attraction", 2, FrameworkPropertyMetadataOptions.None, 0, double.PositiveInfinity, false);

        public double Variation
        {
            get => (double)GetValue(VariationProperty);
            set => SetValue(VariationProperty, value);
        }

        public static readonly DependencyProperty VariationProperty =
            CreateDoubleDp("Variation", 1, FrameworkPropertyMetadataOptions.None, 0, true, 1, true, false);

        #endregion

        protected virtual Point ProcessNewChild(UIElement child, Rect providedBounds)
        {
            return providedBounds.Location;
        }

        protected void ArrangeChild(UIElement child, Rect bounds)
        {
            _mListener.StartListening();

            AnimatingPanelItemData data = (AnimatingPanelItemData)child.GetValue(DataProperty);
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
            double dampening = this.Dampening;
            double attractionFactor = this.Attraction * .01;
            double variation = this.Variation;

            bool shouldChange = false;
            for (int i = 0; i < Children.Count; i++)
            {
                shouldChange = UpdateChildData(
                    (AnimatingPanelItemData)Children[i].GetValue(DataProperty),
                    dampening,
                    attractionFactor,
                    variation) || shouldChange;
            }

            if (!shouldChange)
            {
                _mListener.StopListening();
            }
        }

        private static bool UpdateChildData(AnimatingPanelItemData data, double dampening, double attractionFactor, double variation)
        {
            if (data == null)
            {
                return false;
            }
            else
            {
                Debug.Assert(dampening > 0 && dampening < 1);
                Debug.Assert(attractionFactor > 0 && !double.IsInfinity(attractionFactor));

                attractionFactor *= 1 + (variation * data.RandomSeed - .5);

                Point newLocation;
                Vector newVelocity;

                bool anythingChanged =
                    GeoHelper.Animate(data.Current, data.LocationVelocity, data.Target,
                        attractionFactor, dampening, CTerminalVelocity, CDiff, CDiff,
                        out newLocation, out newVelocity);

                data.Current = newLocation;
                data.LocationVelocity = newVelocity;

                var transformVector = data.Current - data.Target;
                data.Transform.SetToVector(transformVector);

                return anythingChanged;
            }
        }

        private readonly CompositionTargetRenderingListener _mListener = new CompositionTargetRenderingListener();

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
                double value = (double)objValue;

                if (includeMin)
                {
                    if (value < minValue)
                    {
                        return false;
                    }
                }
                else
                {
                    if (value <= minValue)
                    {
                        return false;
                    }
                }
                if (includeMax)
                {
                    if (value > maxValue)
                    {
                        return false;
                    }
                }
                else
                {
                    if (value >= maxValue)
                    {
                        return false;
                    }
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

        private static readonly DependencyProperty DataProperty =
            DependencyProperty.RegisterAttached("Data", typeof(AnimatingPanelItemData), typeof(AnimatingTilePanel));

        private const double CDiff = 0.1;
        private const double CTerminalVelocity = 10000;

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
