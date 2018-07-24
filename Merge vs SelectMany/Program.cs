using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Threading;

namespace Bnaya.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            var xs = Observable.Interval(TimeSpan.FromSeconds(3))
                        .Select(m => (char)('A' + m));
            var ys = Observable.Interval(TimeSpan.FromSeconds(1));

            //var zs = xs.SelectMany(x => ys.Select(y => (x, y)).Take(2));
            //var zs = from x in xs
            //         from y in ys.Take(2)
            //         select (x, y);
            var zs = Observable.Merge(
                            xs.Select(m => m.ToString()), 
                            ys.Select(m => m.ToString()));
            zs.Subscribe(m => Console.Write($"{m}, "));

            while (true)
            {
                Thread.Sleep(100);
                Console.Write(".");
            }

            Console.ReadKey();
        }
    }
}
