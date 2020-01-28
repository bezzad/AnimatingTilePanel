using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace TilePanel
{
    /// <summary>
    ///     A wrapper around an <see cref="IList{TFrom}"/>
    ///     which projects the contents as a read-only <see cref="IList{TTo}"/>.
    /// </summary>
    /// <typeparam name="TFrom">The type of the source <see cref="IList{T}"/>.</typeparam>
    /// <typeparam name="TTo">
    ///     The type of the target <see cref="IList{T}"/>
    ///     Must be assignable from <paramref name="TFrom"/>.
    /// </typeparam>
    /// <remarks>
    ///     Like <see cref="System.Collections.ObjectModel.ReadOnlyCollection{T}"/>, this class
    ///     is a wrapper. Changes to the source collection will
    ///     be reflected.
    /// </remarks>
    public class CastList<TFrom, TTo> : ListBase<TTo>
        where TFrom : TTo
    {
        /// <summary>
        ///     Creats a new <see cref="CastList{TFrom,TTo}"/>.
        /// </summary>
        /// <param name="from">The source collection.</param>
        public CastList(IList<TFrom> from)
        {
            Contract.Requires(from != null);
            _mSource = from;
        }

        /// <summary>
        ///     Gets the element at the specified index.
        /// </summary>
        protected override TTo GetItem(int index)
        {
            return _mSource[index];
        }

        /// <summary>
        ///     Gets the number of contained elements.
        /// </summary>
        public override int Count => _mSource.Count;

        #region Implementation

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_mSource != null);
        }

        private readonly IList<TFrom> _mSource;

        #endregion
    }
}
