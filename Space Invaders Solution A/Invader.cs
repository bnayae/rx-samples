using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Windows;

namespace Bnaya.Samples
{
    public class Invader : Item, IObservable<Item>
    {
        private readonly IObservable<Item> _stream;
        private readonly Func<Size> _boardDimensions;
        public Invader(
            IObservable<int> paceStream,
            Func<Size> boardDimensions,
            double x = 20, double y = 20, double size = 20) :
            base(Kind.Invader, x, y, size)
        {
            _boardDimensions = boardDimensions;
            var stream = paceStream.Do(Advance)
                                .Select(_ => this)
                                .TakeWhile(m => !m.IsHit)
                                .TakeWhile(item => item.Y < _boardDimensions().Height - 30)
                                .Publish();
            stream.Connect();
            _stream = stream;
        }

        private void Advance(int l)
        {
            X += l;
            Size dimension = _boardDimensions();
            if (X > dimension.Width)
            {
                X = Size;
                Y += 30;
            }

        }

        public IDisposable Subscribe(IObserver<Item> observer)
        {
            return _stream.Subscribe(observer);
        }
    }
}
