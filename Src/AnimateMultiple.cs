using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PieceDeal
{
    public class AnimateMultiple : IAnimate
    {
        private IAnimate[] _animates;

        public AnimateMultiple(params IAnimate[] animates)
        {
            _animates = animates;
        }

        public bool IsCompleted { get { return _animates.All(a => a.IsCompleted); } }
        public void Stop()
        {
            foreach (var a in _animates)
                a.Stop();
        }
    }
}
