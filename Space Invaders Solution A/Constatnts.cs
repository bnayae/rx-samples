using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Threading;
using System.Collections.Concurrent;

namespace Bnaya.Samples
{
    public static class Constatnts
    {
        private static readonly ConcurrentDictionary<Color, Brush> _colors = new ConcurrentDictionary<Color, Brush>();

        #region public static Brush GetBrush(Color color)

        public static Brush GetBrush(Color color) 
        {
            Brush b = _colors.GetOrAdd(color, c => 
                {
                    var res = new SolidColorBrush(color);
                    res.Freeze();
                    return res;
                });

            return b;
        }

        #endregion // public static GetBrush GetBrudh(Color color)

        public static ThreadLocal<Random> Rand = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        public static double BorderWidth { get; set; }
        public static double BorderHeight { get; set; }
    }
}
