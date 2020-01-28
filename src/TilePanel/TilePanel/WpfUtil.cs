using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Media;

namespace TilePanel
{
    public static class WpfUtil
    {
        public static void SetToVector(this TranslateTransform translateTransform, Vector vector)
        {
            Contract.Requires(translateTransform != null);
            translateTransform.X = vector.X;
            translateTransform.Y = vector.Y;
        }
    }
}
