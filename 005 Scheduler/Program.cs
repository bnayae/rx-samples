using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bnaya.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = Scheduler.Default;
            //var s = Scheduler.TaskPool;
            //var s = NewThreadScheduler.Default;
            //var s = new EventLoopScheduler();
            var c = new MyComponent(s);

            c.Data.Subscribe(v => Console.WriteLine($"{v}: pool = {Thread.CurrentThread.IsThreadPoolThread} id = {Thread.CurrentThread.ManagedThreadId}"));
            Console.WriteLine("-----------------");
            c.Data.Subscribe(v => Console.WriteLine($"\t{v}: pool = {Thread.CurrentThread.IsThreadPoolThread} id = {Thread.CurrentThread.ManagedThreadId}"));

            Console.ReadKey();
        }
    }
}
