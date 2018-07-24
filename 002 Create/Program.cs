using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
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
            var p = Producer();
            var ys = p.Where(v => v % 2 == 0);
            IDisposable subscription =
                    ys.Subscribe(v => Console.Write($"{v},"),
                         ex => Console.WriteLine(ex),
                         () => Console.WriteLine("|"));
            Console.WriteLine("---------");
            Thread.Sleep(150);
            subscription.Dispose();

            Console.ReadKey();
        }

        #region Producer

        private static IObservable<int> Producer()
        {
            // Can you really cancel subscription?
            var xs = Observable.Create<int>(o =>
            {
                var d = new BooleanDisposable();
                try
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Thread.Sleep(40);
                        if (d.IsDisposed)
                            return d;
                        o.OnNext(i);
                        Console.Write(".");
                    }
                    o.OnCompleted();
                }
                catch (Exception ex)
                {

                    o.OnError(ex);
                }

                return d;
            });

            return xs;
        }

        #endregion // Producer

        #region ProducerAsync

        private static IObservable<int> ProducerAsync()
        {
            var xs = Observable.Create<int>(async (o, ct) =>
            {
                try
                {
                    for (int i = 0; i < 10; i++)
                    {
                        await Task.Delay(40);
                        if (ct.IsCancellationRequested)
                            return;
                        o.OnNext(i);
                        Console.Write(".");
                    }
                    o.OnCompleted();
                }
                catch (Exception ex)
                {

                    o.OnError(ex);
                }
            });

            return xs;
        }

        #endregion // ProducerAsync
    }
}
