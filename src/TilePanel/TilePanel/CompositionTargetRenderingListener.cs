using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Media;

namespace TilePanel
{
    public class CompositionTargetRenderingListener : System.Windows.Threading.DispatcherObject, IDisposable
    {
        public void StartListening()
        {
            RequireAccessAndNotDisposed();

            if (!_mIsListening)
            {
                IsListening = true;
                CompositionTarget.Rendering += compositionTarget_Rendering;
            }
        }

        public void StopListening()
        {
            RequireAccessAndNotDisposed();

            if (_mIsListening)
            {
                IsListening = false;
                CompositionTarget.Rendering -= compositionTarget_Rendering;
            }
        }

        public void WireParentLoadedUnloaded(FrameworkElement parent)
        {
            Contract.Requires(parent != null);
            RequireAccessAndNotDisposed();

            parent.Loaded += delegate (object sender, RoutedEventArgs e)
            {
                StartListening();
            };

            parent.Unloaded += delegate (object sender, RoutedEventArgs e)
            {
                StopListening();
            };
        }

        public bool IsListening
        {
            get => _mIsListening;
            private set
            {
                if (value != _mIsListening)
                {
                    _mIsListening = value;
                    OnIsListeningChanged(EventArgs.Empty);
                }
            }
        }

        public bool IsDisposed
        {
            get
            {
                VerifyAccess();
                return _mDisposed;
            }
        }

        public event EventHandler Rendering;

        protected virtual void OnRendering(EventArgs args)
        {
            RequireAccessAndNotDisposed();

            var handler = Rendering;
            handler?.Invoke(this, args);
        }

        public event EventHandler IsListeningChanged;

        protected virtual void OnIsListeningChanged(EventArgs args)
        {
            var handler = IsListeningChanged;
            handler?.Invoke(this, args);
        }

        public void Dispose()
        {
            RequireAccessAndNotDisposed();
            StopListening();

            Rendering?.GetInvocationList().ForEach(d => Rendering -= (EventHandler) d);
            _mDisposed = true;
        }

        #region Implementation

        [DebuggerStepThrough]
        private void RequireAccessAndNotDisposed()
        {
            VerifyAccess();
            Util.ThrowUnless<ObjectDisposedException>(!_mDisposed, "This object has been disposed");
        }

        private void compositionTarget_Rendering(object sender, EventArgs e)
        {
            OnRendering(e);
        }

        private bool _mIsListening;
        private bool _mDisposed;

        #endregion
    }
}
