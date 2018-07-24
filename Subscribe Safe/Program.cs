using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
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
            var consumer = new Observer();

            var producer = new FaultObservable();

            #region Remarked: var producer = Observable.Create

            //var producer = Observable.Create<long>(o =>
            //        {
            //            throw new NotImplementedException(":(");
            //            return Disposable.Empty;
            //        });

            #endregion // Remarked: var producer = Observable.Create

            producer.Subscribe(consumer);
            //producer.SubscribeSafe(consumer);

            Console.ReadKey();
        }

        private class Observer : IObserver<long>
        {
            public void OnCompleted()
            {
                Console.WriteLine("Completed");
            }

            public void OnError(Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"ERROR: {error.GetBaseException().Message}");
                Console.ResetColor();
            }

            public void OnNext(long value)
            {
                Console.WriteLine(value);
            }
        }
    }
}
