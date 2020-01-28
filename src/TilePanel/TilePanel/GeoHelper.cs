using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Windows;

namespace TilePanel
{
    public static class GeoHelper
    {
        [Pure]
        public static bool IsValid(this double value)
        {
            return !double.IsInfinity(value) && !double.IsNaN(value);
        }

        [Pure]
        public static bool IsValid(this Point value)
        {
            return value.X.IsValid() && value.Y.IsValid();
        }

        [Pure]
        public static bool IsValid(this Size value)
        {
            return value.Width.IsValid() && value.Height.IsValid();
        }

        [Pure]
        public static bool IsValid(this Vector value)
        {
            return value.X.IsValid() && value.Y.IsValid();
        }

        public static bool Animate(
            Point currentValue, Vector currentVelocity, Point targetValue,
            double attractionFator, double dampening,
            double terminalVelocity, double minValueDelta, double minVelocityDelta,
            out Point newValue, out Vector newVelocity)
        {
            Debug.Assert(currentValue.IsValid());
            Debug.Assert(currentVelocity.IsValid());
            Debug.Assert(targetValue.IsValid());

            Debug.Assert(dampening.IsValid());
            Debug.Assert(dampening > 0 && dampening < 1);

            Debug.Assert(attractionFator.IsValid());
            Debug.Assert(attractionFator > 0);

            Debug.Assert(terminalVelocity > 0);

            Debug.Assert(minValueDelta > 0);
            Debug.Assert(minVelocityDelta > 0);

            var diff = targetValue.Subtract(currentValue);

            if (diff.Length > minValueDelta || currentVelocity.Length > minVelocityDelta)
            {
                newVelocity = currentVelocity * (1 - dampening);
                newVelocity += diff * attractionFator;
                if (currentVelocity.Length > terminalVelocity)
                {
                    newVelocity *= terminalVelocity / currentVelocity.Length;
                }

                newValue = currentValue + newVelocity;

                return true;
            }
            else
            {
                newValue = targetValue;
                newVelocity = new Vector();
                return false;
            }
        }

        public static Vector Subtract(this Point point, Point other)
        {
            Contract.Requires(point.IsValid());
            Contract.Requires(other.IsValid());
            Contract.Ensures(Contract.Result<Vector>().IsValid());
            return new Vector(point.X - other.X, point.Y - other.Y);
        }
    }
}
