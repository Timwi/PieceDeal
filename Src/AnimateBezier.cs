using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace PieceDeal
{
    public class AnimateBézier : AnimateBase
    {
        private Point[] _controlPoints;
        public event Action<Point> Action;

        public AnimateBézier(Point[] controlPoints, TimeSpan duration)
            : base(duration)
        {
            if (controlPoints == null || controlPoints.Length != 4)
                throw new ArgumentException("Number of control points for Bézier curve must be exactly 4.", "controlPoints");
            _controlPoints = controlPoints;
        }

        public AnimateBézier(Point[] controlPoints, TimeSpan duration, Action<Point> action)
            : this(controlPoints, duration)
        {
            Action += action;
        }

        protected override void Act(double time)
        {
            if (Action != null)
            {
                var t = 1 - time;
                var x = _controlPoints[0].X * t * t * t + _controlPoints[1].X * 3 * t * t * time + _controlPoints[2].X * 3 * t * time * time + _controlPoints[3].X * time * time * time;
                var y = _controlPoints[0].Y * t * t * t + _controlPoints[1].Y * 3 * t * t * time + _controlPoints[2].Y * 3 * t * time * time + _controlPoints[3].Y * time * time * time;
                Action(new Point(x, y));
            }
        }
    }
}
