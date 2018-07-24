#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

#endregion // Using

namespace Bnaya.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            IObservable<int> xs = Producer();

            #region Option 1

            var ys = from win in xs.Window(TimeSpan.FromSeconds(2))
                     from item in win.Aggregate(new Acc(), Acc.AggregateMinMax)
                     select item;

            #endregion // Option 1

            #region Option 2

            //var ys = from win in xs.Window(TimeSpan.FromSeconds(2))
            //         let w = win.Publish().RefCount()
            //         from result in new[]
            //            {
            //                w.Min(),
            //                w.Max(),
            //            }.Zip()
            //         select new { Min = result[0], Max = result[1] };

            #endregion // Option 2

            ys.Subscribe(v => Console.WriteLine("{0} - {1}", v.Min, v.Max));

            Console.WriteLine("Start");
            Console.ReadKey();

        }

        #region Producer

        private static Random _rnd = new Random(Guid.NewGuid().GetHashCode());
        private static IObservable<int> Producer()
        {
            return Observable.Generate(
                10000,
                i => i > 0,
                i => i + _rnd.Next(-10, 10),
                i => i,
                i => TimeSpan.FromMilliseconds(1));
        }

        #endregion // Producer

        #region Acc

        private class Acc
        {
            public int? Min { get; set; }
            public int? Max { get; set; }

            public static Acc AggregateMinMax(Acc ac, int i)
            {
                var max = ac.Max ?? int.MinValue;
                max = max > i ? max : i;
                var min = ac.Min ?? int.MaxValue;
                min = min < i ? min : i;

                ac.Min = min;
                ac.Max = max;

                return ac;
            }
        }

        #endregion // Acc
    }
}
