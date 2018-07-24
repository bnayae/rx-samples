#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using System.Reactive;
using System.Reactive.Linq;

#endregion // Using

namespace Bnaya.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var scheduler = new PrisonScheduler(); // "III" = 3 minutes

            var xs = Observable.Interval(TimeSpan.FromMinutes(1), scheduler).Take(10);
            xs.Subscribe(Console.WriteLine, () => Console.WriteLine("Complete"));

            Console.WriteLine("Shift 2 minutes");
            scheduler.AdvanceBy(2);
            Thread.Sleep(2000);
            Console.WriteLine("Go to minute 5");
            scheduler.AdvanceTo("IIIII");
            Thread.Sleep(2000);
            Console.WriteLine("Shift 5 minutes");
            scheduler.AdvanceBy(5);

            Console.ReadKey();
        }
    }
}
