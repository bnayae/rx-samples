using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Bnaya.Samples
{
    public class CustomCanvas : Control
    {
        private Brush _invaderBrush = Constatnts.GetBrush(Colors.Crimson);
        private Brush _defenderBrush = Constatnts.GetBrush(Colors.CadetBlue);
        private Brush _penBrush = Constatnts.GetBrush(Colors.Black);
        private Pen _pen;

        public CustomCanvas()
        {
            _invaderBrush.Freeze();
            _penBrush.Freeze();
            _pen = new Pen(_penBrush, 2);
            _pen.Freeze();

            var scd = new SynchronizationContextScheduler(SynchronizationContext.Current);

            RenderLoop(scd);
        }

        public ViewModel VM { get; set; }
        public TimeSpan FrameRate { get; set; } = TimeSpan.FromSeconds(0.05);//1.0 / 60);

        #region RenderLoop

        private void RenderLoop(SynchronizationContextScheduler scd)
        {
            Observable.Interval(FrameRate)
                    .ObserveOn(scd)
                    .Subscribe(m => this.InvalidateVisual());

            var contextChanges = Observable.FromEventPattern<DependencyPropertyChangedEventArgs>(
                            this, nameof(DataContextChanged))
                            .Select(m => m.EventArgs.NewValue as ViewModel);

        }

        #endregion // RenderLoop

        #region MeasureOverride

        protected override Size MeasureOverride(Size constraint)
        {
            var result = base.MeasureOverride(constraint);
            if (constraint.IsEmpty)
                return result;
            Constatnts.BorderHeight = constraint.Height;
            Constatnts.BorderWidth = constraint.Width;
            if (VM != null)
            {
                Item defender = VM.Defender;
                if (defender.X < Constatnts.BorderWidth || defender.X < 0)
                    defender.X = Constatnts.BorderWidth / 2;
                defender.Y = Constatnts.BorderHeight - defender.Size;
            }
            return result;
        }

        #endregion // MeasureOverride

        #region OnRender

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (Width == 0 || Height == 0)
                return;

            if (VM == null)
                return;

            //var invaders = new GeometryGroup();
            foreach (var invader in VM.InvadersState)
            {
                int size = (int)invader.Size;
                int x = (int)invader.X;
                int y = (int)invader.Y;
                var rect = new Rect(x, y, size, size);
                //invaders.Children.Add(new RectangleGeometry(rect, 4, 4));
                dc.DrawRectangle(_invaderBrush, _pen, rect);
            }
            //invaders.Freeze();
            //dc.DrawGeometry(_invaderBrush, _pen, invaders);

            foreach (var missile in VM.DefenderMissilesState)
            {
                int size = (int)missile.Size;
                int x = (int)missile.X;
                int y = (int)missile.Y;
                dc.DrawEllipse(_defenderBrush, _pen, new Point(x, y), size / 5, size / 4);
            }

            int dx = (int)VM.Defender.X;
            int dy = (int)VM.Defender.Y;
            int dsize = (int)VM.Defender.Size;
            dc.DrawRectangle(_defenderBrush, _pen, new Rect(dx, dy, dsize, dsize / 4));
        }
 
        #endregion // OnRender
   }
}
