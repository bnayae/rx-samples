using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _013_Windows_and_Aggregation
{
    class Program
    {
        static void Main(string[] args)
        {

            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                        .Select(m => m % 2 == 0 ? m : -m);
            //var ys = xs.Window(4)
            //            .SelectMany(w =>
            //            {
            //                IObservable<long> min = w.Min();
            //                IObservable<long> max = w.Max();
            //                return Observable.Zip(min, max, (a, b) => (Min:a, Max:b));
            //            });

            var ys = from w in xs.Window(4)
                     let min = w.Min()
                     let max = w.Max()
                     from zip in Observable.Zip(min, max, (a, b) => (Min: a, Max: b))
                     select zip;

            ys.Subscribe(m => Console.WriteLine($"{m.Min}:{m.Max},"));


            Console.ReadKey();
        }
    }
}
