using System;
using System.Collections.Generic;
using System.Linq;
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

            var input1 = Observable.Interval(TimeSpan.FromSeconds(0.5))
                .Do(v => Console.WriteLine("\t{0}", v))
                .Publish().RefCount();
            var input2 = Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
                .Select(v => new string('*', (int)v + 1))
                .Do(v => Console.WriteLine("\t{0}", v));
            var input3 = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(2))
                .Select(v => new string('#', (int)v + 1))
                .Do(v => Console.WriteLine("\t{0}", v));

            var other = Observable.Interval(TimeSpan.FromSeconds(7))
                .Select(v => "Flush")
                .Do(v => Console.WriteLine("\t\t{0}", v));

            var pattern = input1.And(input2).And(input3);
            var pattern1 = input1.And(input2);
            var pattern2 = input1.And(input3);

            var source =
                Observable.When(
                            pattern.Then((a, b, c) => String.Format("{0} - {1} - {2}", a, b, c)),
                            input1.And(other).Then((i, f) => f));//,
            //pattern1.Then((a, b) => String.Format("{0} - {1}", a, b)),
            //pattern2.Then((a, b) => String.Format("{0} - {1}", a, b)));


            source.Subscribe(Console.WriteLine);

            Console.ReadKey();
        }
    }
}
