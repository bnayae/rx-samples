using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Diagnostics;

namespace Bnaya.Samples
{
    /// <summary>
    /// Single item data
    /// </summary>
    [DebuggerDisplay("X = {X}, Y = {Y}")]
    public class Item 
    {
        private static int _nextId;

        public const int DEFAULT_SIZE = 30;

        #region Ctor

        public Item(
            Kind kind, 
            double x = DEFAULT_SIZE,
            double y = DEFAULT_SIZE,
            double size = DEFAULT_SIZE)
        {
            Id = _nextId++;
            Kind = kind;
            Size = size;
            X = x;
            Y = y;
            Color = Constatnts.GetBrush(Colors.IndianRed);
            IsHit = false;
            Initialize(x, y);
        }

        #endregion // Ctor

        #region Properties

        public int Id { get; }
        public double Size { get; }
        public bool IsHit { get; private set; }
        public Kind Kind { get; private set; }
        public double X { get; internal set; }
        public double Y { get; internal set; }

        public Brush Color { get;  }

        #endregion // Properties

        #region Initialize

        public void Initialize(double x = DEFAULT_SIZE, double y = DEFAULT_SIZE)
        {
            Y = y;
            X = x;
            IsHit = false;
        }

        #endregion // Initialize

        public void Hit()
        {
            IsHit = true;
        }

        #region IsIntersect

        public bool IsIntersect(Item other)
        {
            if (X + Size < other.X)  // --x===---o.x---
                return false;
            if (Y + Size < other.Y)  // --y===---o.y---
                return false;
            if (other.X < X)          // --o.x---x------
                return false;
            if (other.Y < Y)          // --o.y---y------
                return false;

            return true;
        }

        #endregion // IsIntersect
    }
}
