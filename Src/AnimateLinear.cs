using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PieceDeal
{
    public class AnimateLinear : AnimateBase
    {
        public double From { get; set; }
        public double To { get; set; }
        public event Action<double> Action;

        public AnimateLinear(double from, double to, TimeSpan duration)
            : base(duration)
        {
            From = from;
            To = to;
        }

        public AnimateLinear(double from, double to, TimeSpan duration, Action<double> action)
            : this(from, to, duration)
        {
            Action += action;
        }

        protected override void Act(double time)
        {
            if (Action != null)
                Action(From + time * (To - From));
        }
    }

    public class AnimateLinearXY : AnimateBase
    {
        public double FromX { get; set; }
        public double ToX { get; set; }
        public double FromY { get; set; }
        public double ToY { get; set; }
        public event Action<double, double> Action;

        public AnimateLinearXY(double fromX, double toX, double fromY, double toY, TimeSpan duration)
            : base(duration)
        {
            FromX = fromX;
            ToX = toX;
            FromY = fromY;
            ToY = toY;
        }

        public AnimateLinearXY(double fromX, double toX, double fromY, double toY, TimeSpan duration, Action<double, double> action)
            : this(fromX, toX, fromY, toY, duration)
        {
            Action += action;
        }

        protected override void Act(double time)
        {
            if (Action != null)
                Action(FromX + time * (ToX - FromX), FromY + time * (ToY - FromY));
        }
    }
}
