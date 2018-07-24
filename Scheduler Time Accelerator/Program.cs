using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace Bnaya.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            var scheduler = new TimeAccelerateScheduler(1 / 60.0); // schedule to run 60 times faster

            var xs = Observable.Interval(TimeSpan.FromMinutes(1), scheduler)
                               .Take(5);
            xs.Subscribe(m => Console.Write($"{m}, "), 
                        () => Console.WriteLine("Complete"));


            Console.ReadKey();
        }
    }
}
