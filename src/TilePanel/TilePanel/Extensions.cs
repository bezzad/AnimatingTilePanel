using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace TilePanel
{
    /// <summary>
    ///     Contains general purpose extention methods.
    /// </summary>
    public static class Extensions
    {
        public static IComparer<T> ToComparer<T>(this Func<T, T, int> compareFunction)
        {
            Contract.Requires(compareFunction != null);
            return new FuncComparer<T>(compareFunction);
        }

        public static IComparer<T> ToComparer<T>(this Comparison<T> compareFunction)
        {
            Contract.Requires(compareFunction != null);
            return new ComparisonComparer<T>(compareFunction);
        }

        [Pure]
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return str == null || str.Trim().Length == 0;
        }

        public static string DoFormat(this string source, params object[] args)
        {
            Contract.Requires(source != null);
            Contract.Requires(args != null);
            return string.Format(source, args);
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this ICustomAttributeProvider attributeProvider, bool inherit) where T : Attribute
        {
            Contract.Requires(attributeProvider != null);
            return attributeProvider.GetCustomAttributes(typeof(T), inherit).Cast<T>();
        }

        [Pure]
        public static bool HasPublicInstanceProperty(this IReflect type, string name)
        {
            Contract.Requires(type != null);
            return type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance) != null;
        }

        #region impl
        private class FuncComparer<T> : IComparer<T>
        {
            public FuncComparer(Func<T, T, int> func)
            {
                Contract.Requires(func != null);
                _mFunc = func;
            }

            public int Compare(T x, T y)
            {
                return _mFunc(x, y);
            }

            private readonly Func<T, T, int> _mFunc;
        }

        private class ComparisonComparer<T> : IComparer<T>
        {
            public ComparisonComparer(Comparison<T> func)
            {
                Contract.Requires(func != null);
                _mFunc = func;
            }

            public int Compare(T x, T y)
            {
                return _mFunc(x, y);
            }

            private readonly Comparison<T> _mFunc;
        }

        private class FuncEqualityComparer<T> : IEqualityComparer<T>
        {
            public FuncEqualityComparer(Func<T, T, bool> func)
            {
                Contract.Requires(func != null);
                _mFunc = func;
            }
            public bool Equals(T x, T y)
            {
                return _mFunc(x, y);
            }

            public int GetHashCode(T obj)
            {
                return 0; // this is on purpose. Should only use function...not short-cut by hashcode compare
            }

            [ContractInvariantMethod]
            void ObjectInvariant()
            {
                Contract.Invariant(_mFunc != null);
            }

            private readonly Func<T, T, bool> _mFunc;
        }
        #endregion
    }
}
