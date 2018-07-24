using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hot_vs_Cold
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            SimpleColdHot();
            //AutoColdHot();
            //ScopedColdHotMore();

            Console.ReadKey();
        }

        private static void SimpleColdHot()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5)).Publish();

            xs.Subscribe(m => Console.WriteLine(m));
            IDisposable connection = xs.Connect();
            Thread.Sleep(2000);
            xs.Subscribe(m => Console.WriteLine($"\t{m}"));

        }

        private static void AutoColdHot()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5));
            var ys = xs.Publish();
            var zs = ys.RefCount();

            IDisposable subscriptionA = zs.Subscribe(m => Console.WriteLine(m));

            Thread.Sleep(2000);
            IDisposable subscriptionB = zs.Subscribe(m => Console.WriteLine($"\t{m}"));
            Thread.Sleep(1200);
            subscriptionA.Dispose();
            Thread.Sleep(2000);
            subscriptionB.Dispose();

            IDisposable subscriptionC = zs.Subscribe(m => Console.WriteLine(m));
        }

        private static void ScopedColdHotMore()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5));
            var zs = xs.Publish(hot =>
            {
                return hot.TakeUntil(hot.Where(i => i == 6));
            });

            IDisposable subscriptionA = zs.Subscribe(m => Console.WriteLine(m));

            Thread.Sleep(2000);
            IDisposable subscriptionB = zs.Subscribe(m => Console.WriteLine($"\t{m}"));
            Thread.Sleep(1200);
            subscriptionA.Dispose();
            Thread.Sleep(2000);
            subscriptionB.Dispose();

            IDisposable subscriptionC = zs.Subscribe(m => Console.WriteLine(m));
        }
    }
}
