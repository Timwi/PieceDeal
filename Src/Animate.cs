using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Threading;

namespace PieceDeal
{
    public interface IAnimate
    {
        bool IsCompleted { get; }
        void Stop();
    }

    public abstract class AnimateBase : IAnimate
    {
        private Timer _timer;
        private DateTime _started;
        private TimeSpan _duration;
        private Dispatcher _dispatcher;

        public event Action Completed;
        public bool IsCompleted { get { return _timer == null; } }

        public AnimateBase(TimeSpan duration)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _started = DateTime.UtcNow;
            _duration = duration;
            _timer = new Timer { AutoReset = true, Enabled = true, Interval = 1 };
            _timer.Elapsed += elapseInThread;
        }

        public void Stop()
        {
            if (_timer == null)
                return;
            _timer.Enabled = false;
            _timer.Dispose();
            _timer = null;
        }

        protected abstract void Act(double time);

        private void elapseInThread(object sender, ElapsedEventArgs e)
        {
            _dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(elapse));
        }

        private void elapse()
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan interval = now - _started;

            if (interval >= _duration)
            {
                // Time's up
                Act(1);
                Stop();
                if (Completed != null)
                    Completed();
                return;
            }

            // Time's not up yet; animate
            Act((double) interval.Ticks / _duration.Ticks);
        }
    }

    public static class Animate
    {
        public static void Later(TimeSpan when, Action action)
        {
            double ms = when.TotalMilliseconds;
            var dispatcher = Dispatcher.CurrentDispatcher;
            if (ms == 0)
                dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
            else
            {
                Timer t = new Timer { AutoReset = false, Enabled = true, Interval = when.TotalMilliseconds };
                t.Elapsed += (s, e) => dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
            }
        }

        public static void LaterMs(double milliseconds, Action action)
        {
            Later(TimeSpan.FromMilliseconds(milliseconds), action);
        }

        public static void LaterS(double seconds, Action action)
        {
            Later(TimeSpan.FromSeconds(seconds), action);
        }
    }
}
