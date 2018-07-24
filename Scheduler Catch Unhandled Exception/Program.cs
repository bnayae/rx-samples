using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
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
            IScheduler scd = Scheduler.Default;
            //IScheduler scd = NewThreadScheduler.Default;
            //IScheduler scd = ThreadPoolScheduler.Instance;
            //IScheduler scd = TaskPoolScheduler.Default;

            //scd = scd.Catch<Exception>(ex =>
            //            {
            //                Console.WriteLine("Opss!: {0}", ex.GetBaseException().Message);
            //                return true; // indicate handling of the exception
            //            });

            var xs = Observable.Interval(TimeSpan.FromSeconds(1), scd);

            xs.Subscribe(v => Console.WriteLine(v));
            xs.Subscribe(v => { throw new Exception("Not Good"); },
                         ex => Console.WriteLine($"Fault: {ex.GetBaseException().Message}"));
            xs.Subscribe(v => Console.WriteLine(v));

            Console.ReadKey();
        }
    }
}
