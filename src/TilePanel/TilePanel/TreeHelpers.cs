using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace TilePanel
{
    public static class TreeHelpers
    {
        public static T FindParent<T>(this DependencyObject obj) where T : DependencyObject
        {
            return obj.GetAncestors().OfType<T>().FirstOrDefault();
        }

        /// <remarks>Includes element.</remarks>
        public static IEnumerable<DependencyObject> GetAncestors(this DependencyObject element)
        {
            Contract.Requires(element != null);
            Contract.Ensures(Contract.Result<IEnumerable<DependencyObject>>() != null);
            do
            {
                yield return element;
                element = VisualTreeHelper.GetParent(element);
            } while (element != null);
        }
    }
}
