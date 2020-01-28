using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace TilePanel
{
    public static class CollectionExtensions
    {
        /// <summary>
        ///     Performs the specified <paramref name="action"/>
        ///     on each element of the specified <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The sequence to which is applied the specified <paramref name="action"/>.</param>
        /// <param name="action">The action applied to each element in <paramref name="source"/>.</param>
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            Contract.Requires(source != null);
            Contract.Requires(action != null);

            foreach (var item in source)
            {
                action(item);
            }
        }
    }
}
