using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bnaya.Samples
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            EnableDragging(button1);
            EnableDragging(button2);
            EnableDragging(button3);
            EnableDragging(textBox1);
            EnableDragging(this);
        }

        private void EnableDragging(Control c)
        {
            // Long way, but strongly typed. 
            var downs = from down in Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                            eh => new MouseEventHandler(eh),
                            eh => c.MouseDown += eh,
                            eh => c.MouseDown -= eh)
                        select new { down.EventArgs.X, down.EventArgs.Y };

            // Short way. 
            var moves = from move in
                            Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                                 eh => new MouseEventHandler(eh),
                                    eh => c.MouseMove += eh,
                                    eh => c.MouseMove -= eh)
                        select new { move.EventArgs.X, move.EventArgs.Y };

            //var moves = from move in
            //                Observable.FromEventPattern<MouseEventArgs>(c, nameof(c.MouseMove))
            //            select new { move.EventArgs.X, move.EventArgs.Y };

            var ups = Observable.FromEventPattern<MouseEventArgs>(c, nameof(MouseUp));

            //var drags = downs.SelectMany(d => moves.TakeUntil(ups))
            //                 .Select(move => new Point { X = move.X - down.X, Y = move.Y - down.Y });
            var drags = from down in downs // for-each mouse down
                        from move in moves.TakeUntil(ups) // produce mouse move, until mouse up
                        select new Point { X = move.X - down.X, Y = move.Y - down.Y };

            drags.Subscribe(drag => c.SetBounds(c.Location.X + drag.X, c.Location.Y + drag.Y, 0, 0, BoundsSpecified.Location));
        }
    }
}
