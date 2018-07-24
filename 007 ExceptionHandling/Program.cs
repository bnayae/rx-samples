#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading.Tasks;

#endregion // Using

namespace Bnaya.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = new EventLoopScheduler();
            IObservable<int> observable = CreateStream(-2).ObserveOn(s);

            #region Hide

            //observable = observable.Retry(3);
            //observable = observable.Catch((ArgumentException ex) => CreateStream(20));
            //observable = observable.Catch(CreateStream(10));
            //observable = observable.Finally(() => Console.WriteLine("Finally"));

            #endregion // Hide


            observable.Subscribe(item =>
                {
                    Console.WriteLine(item);
                    if (item == 1)
                        throw new IndexOutOfRangeException("X");
                },
                (ex) => Console.WriteLine("ERROR: {0}", ex.Message),
                () => Console.WriteLine("Complete"));

            Console.ReadLine();
        }

        #region CreateStream

        private static IObservable<int> CreateStream(int startWith)
        {
            IObservable<int> observable = Observable.Create<int>(async (observer, ct) =>
            {
                for (int i = startWith; i < startWith + 10; i++)
                {
                    await Task.Delay(500);

                    #region Validation

                    if (ct.IsCancellationRequested)
                        return Disposable.Empty;

                    if (i == 3)
                    {
                        observer.OnError(new ArgumentException("cannot handle 3"));
                        return Disposable.Empty;
                    }

                    #endregion // Validation

                    observer.OnNext(i);
                }

                observer.OnCompleted();


                return Disposable.Empty;
            });
            return observable;
        }

        #endregion // CreateStream
    }
}
