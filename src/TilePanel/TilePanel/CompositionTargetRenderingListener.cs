using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TilePanel
{
    public class CompositionTargetRenderingListener :
#if !SILVERLIGHT
 System.Windows.Threading.DispatcherObject,
#endif
 IDisposable
    {
        public CompositionTargetRenderingListener() { }

        public void StartListening()
        {
            RequireAccessAndNotDisposed();

            if (!_mIsListening)
            {
                IsListening = true;
                CompositionTarget.Rendering += compositionTarget_Rendering;
#if FRAME_RATE
        m_startTicks = Environment.TickCount;
        m_count = 0;
#endif
            }
        }

        public void StopListening()
        {
            RequireAccessAndNotDisposed();

            if (_mIsListening)
            {
                IsListening = false;
                CompositionTarget.Rendering -= compositionTarget_Rendering;
#if FRAME_RATE
        var ticks = Environment.TickCount - m_startTicks;
        var seconds = ticks / 1000.0;
        Debug.WriteLine("Seconds: {0}, Count: {1}, Frame rate: {2}", seconds, m_count, m_count/seconds);
#endif
            }
        }

#if !WP7
        public void WireParentLoadedUnloaded(FrameworkElement parent)
        {
            Contract.Requires(parent != null);
            RequireAccessAndNotDisposed();

            parent.Loaded += delegate (object sender, RoutedEventArgs e)
            {
                this.StartListening();
            };

            parent.Unloaded += delegate (object sender, RoutedEventArgs e)
            {
                this.StopListening();
            };
        }
#endif

        public bool IsListening
        {
            get => _mIsListening;
            private set
            {
                if (value != _mIsListening)
                {
                    _mIsListening = value;
                    OnIsListeneningChanged(EventArgs.Empty);
                }
            }
        }

        public bool IsDisposed
        {
            get
            {
#if !SILVERLIGHT
                VerifyAccess();
#endif
                return _mDisposed;
            }
        }

        public event EventHandler Rendering;

        protected virtual void OnRendering(EventArgs args)
        {
            RequireAccessAndNotDisposed();

            EventHandler handler = Rendering;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public event EventHandler IsListeningChanged;

        protected virtual void OnIsListeneningChanged(EventArgs args)
        {
            var handler = IsListeningChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public void Dispose()
        {
            RequireAccessAndNotDisposed();
            StopListening();

            Rendering
              .GetInvocationList()
              .ForEach(d => Rendering -= (EventHandler)d);

            _mDisposed = true;
        }

        #region Implementation

        [DebuggerStepThrough]
        private void RequireAccessAndNotDisposed()
        {
#if !SILVERLIGHT
            VerifyAccess();
#endif
            Util.ThrowUnless<ObjectDisposedException>(!_mDisposed, "This object has been disposed");
        }

        private void compositionTarget_Rendering(object sender, EventArgs e)
        {
#if FRAME_RATE
      m_count++;
#endif
            OnRendering(e);
        }

        private bool _mIsListening;
        private bool _mDisposed;

#if FRAME_RATE
    private int m_count = 0;
    private int m_startTicks;
#endif

        #endregion
    }
}
