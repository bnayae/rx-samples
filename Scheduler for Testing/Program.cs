#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;

#endregion // Using

namespace Bnaya.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var scheduler = new TestScheduler();

            var xs = Observable.Interval(TimeSpan.FromMinutes(1), scheduler).Take(10);
            xs.Subscribe(m => Console.Write($"{m}, "), () => Console.WriteLine("Complete"));

            long singleTimeUnit = TimeSpan.FromMinutes(1).Ticks;
            Console.WriteLine("Shift 1 minute");
            scheduler.AdvanceBy(singleTimeUnit);
            Console.WriteLine("\r\n\r\nShift 1 minute");
            scheduler.AdvanceBy(singleTimeUnit);
            Console.WriteLine("\r\n\r\nShift 2 minute");
            scheduler.AdvanceBy(singleTimeUnit * 2);
            Console.WriteLine("\r\n\r\nGo to minute 10");
            scheduler.AdvanceTo(singleTimeUnit * 10);

            Console.ReadKey();
        }
    }
}
