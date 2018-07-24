using System;
using System.Collections.Generic;
using System.IO;
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
        private const string FOLDER = @"Data";
        static void Main(string[] args)
        {
            Console.WriteLine("Start");

            var xs = new FileObservable(FOLDER);

            var ys = xs.Where(v => v != 'a')
                .Throttle(TimeSpan.FromSeconds(2))
                .DistinctUntilChanged();

            ys.Subscribe(v => Console.Write($"{v}--"),
                         ex => Console.Write("X"),
                         () => Console.Write("|"));
            Console.ReadKey();
        }
    }
}
