using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _011_Replay
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            var xs = Observable.Interval(TimeSpan.FromSeconds(1))
                            .Select(m => ('A', m));
            var ys = Observable.Interval(TimeSpan.FromSeconds(1.7))
                            .Select(m => ('B', m));
            var bs = new BehaviorSubject<(char, long)>((' ', 0));
            xs.Subscribe(bs);
            ys.Subscribe(bs);

            bs.Subscribe(m => Console.Write($"{m},"));

            var tmr = new Timer(s => Console.WriteLine($"#{bs.Value}"), null, 2500, 2500);

            Replay();
            Console.ReadKey();
        }

        private static void Replay()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(1));
            IConnectableObservable<long> ys = xs.Replay(TimeSpan.FromSeconds(2));
            IDisposable connection = ys.Connect();
            Thread.Sleep(4200);
            ys.Subscribe(m => Console.Write($"{m},"));
            Thread.Sleep(3000);
            connection.Dispose();
        }
    }
}
