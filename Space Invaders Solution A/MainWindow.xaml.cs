using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bnaya.Samples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly static TimeSpan DEFENDER_SPEED = TimeSpan.FromMilliseconds(30);

        public MainWindow()
        {
            InitializeComponent();

            var down = Observable.FromEventPattern<KeyEventArgs>(this, nameof(KeyDown))
                                 .Select(arg => arg.EventArgs.Key);
            var up = Observable.FromEventPattern<KeyEventArgs>(this, nameof(KeyUp))
                                 .Select(arg => arg.EventArgs.Key);

            Func<Key, bool> isMove = k => k == Key.Right || k == Key.Left;
            var moves = from k in down.Where(isMove)
                        from i in Observable.Interval(DEFENDER_SPEED, NewThreadScheduler.Default)
                                            .TakeUntil(up.Where(isMove))
                        select MapAction(k);

            var fires = down.Where(k => k == Key.Space)
                            .Select(_ => DefenderAction.Fire);
            var keys = Observable.Merge(moves, fires);
            var vm = new ViewModel(keys, _canvas.FrameRate);
            _canvas.VM = vm;
        }

        private DefenderAction MapAction(Key key)
        {
            switch (key)
            {
                case Key.Right:
                    return DefenderAction.Right;
                case Key.Left:
                    return DefenderAction.Left;
                case Key.Space:
                    return DefenderAction.Fire;
                default:
                    return DefenderAction.None;
            }
        }

    }

}
