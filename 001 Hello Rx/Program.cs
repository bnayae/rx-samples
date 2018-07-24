using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnaya.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");

            // 0--1--2--3--4--5--6--7--8--9-->
            // ---1--2-----4--5-----7--8----->
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                               .Where(m => m % 3 != 0);

            #region Remarked

            //var xs = Observable.Create<int>(obs =>
            //{
            //    Task.Run(async () =>
            //    {
            //        var rnd = new Random();
            //        for (int i = 0; i < 100; i++)
            //        {
            //            await Task.Delay(rnd.Next(1, 1400)).ConfigureAwait(false);
            //            obs.OnNext(i);
            //        }
            //    });

            //    return Disposable.Empty;
            //});

            #endregion // Remarked

            xs
                .Throttle(TimeSpan.FromMilliseconds(800))
                .Subscribe(v => Console.Write($"{v},"));

            #region Remarked

            //xs
            //    .TimeInterval()
            //    .Do(v => Console.Write($"[{v.Interval.TotalMilliseconds:N0}] "))
            //    .Throttle(TimeSpan.FromMilliseconds(500))
            //    .TimeInterval()
            //    .Do(v => Console.Write($"<{v.Interval.TotalMilliseconds:N0}> "))
            //    .Subscribe(v => Console.WriteLine(v.Value.Value));

            #endregion // Remarked

            Console.ReadKey();
        }
    }
}
