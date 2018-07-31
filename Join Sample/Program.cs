using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Join_Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            //JoinSample();
            GroupJoinSample();

            Console.ReadKey();

        }

        private static void JoinSample()
        {
            var xs = Observable.Timer(TimeSpan.FromSeconds(1.5),
                                      TimeSpan.FromSeconds(4))
                               .Select(m => (Char)('A' + m % 26));
            var ys = Observable.Interval(TimeSpan.FromSeconds(1))
                               .Publish().RefCount();

            var joined = xs.Publish(hot =>
            {
                return hot.Join(ys,
                             x => hot, // Observable.Timer(TimeSpan.FromMinutes(1)),
                             y => ys, //.Skip(1), //  Observable.Empty<Unit>(),
                             (x, y) => (X: x, Y: y));
            });

            joined.Subscribe(m => Console.WriteLine(m));
        }

        private static void GroupJoinSample()
        {
            var xs = Observable.Timer(TimeSpan.FromSeconds(1.5),
                                      TimeSpan.FromSeconds(4))
                               .Select(m => (Char)('A' + m % 26))
                               .Publish().RefCount(); 
            var ys = Observable.Interval(TimeSpan.FromSeconds(1))
                               .Publish().RefCount();

            var joined = from g in xs.GroupJoin(ys,
                             x => xs, // Observable.Timer(TimeSpan.FromMinutes(1)),
                             y => ys, //.Skip(1), //  Observable.Empty<Unit>(),
                             (x, y) => y.Aggregate(x.ToString(),
                                                (acc, val) => $"{acc}, {val}"))
                         from item in g  
                         select item;
            

            joined.Subscribe(m => Console.WriteLine(m));
        }
    }
}

