using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hot_Cold_Trap
{
    class Program
    {
        static void Main(string[] args)
        {
            var rnd = new Random();
            //var x = Enumerable.Range(0, 10)
            //                  .Select(m => rnd.Next(0, 10));
            //                  //.ToArray();
            //var a = x.Except(x);
            //var b = a.ToArray();

            var xs = Observable.Interval(TimeSpan.FromSeconds(0.2))
                               .Select(m => rnd.Next(0, 20))
                               .Publish().RefCount();
            var closing = xs.Where(m => m % 10 == 0);
            var zs = xs.Buffer(closing);

            zs.Subscribe(m => Console.WriteLine(string.Join(", ", m)));

            Console.ReadKey();
        }
    }
}
