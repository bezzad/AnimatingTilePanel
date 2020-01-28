using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace TilePanel
{
    /// <summary>
    ///     Contains general helper methods.
    /// </summary>
    public static class Util
    {
        public static Random Rnd
        {
            get
            {
                Contract.Ensures(Contract.Result<Random>() != null);
                var r = (Random)SRandom.Target;
                if (r == null)
                {
                    SRandom.Target = r = new Random();
                }
                return r;
            }
        }

        [DebuggerStepThrough]
        public static void ThrowUnless(bool truth, string message = null)
        {
            ThrowUnless<Exception>(truth, message);
        }

        [DebuggerStepThrough]
        public static void ThrowUnless<TException>(bool truth, string message) where TException : Exception
        {
            if (!truth)
            {
                throw InstanceFactory.CreateInstance<TException>(message);
            }
        }

        private static readonly WeakReference SRandom = new WeakReference(null);
    }
}
