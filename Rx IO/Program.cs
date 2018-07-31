using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Rx_IO
{
    class Program
    {
        private const string URL = "https://source.unsplash.com/random/1000x1000";

        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            if (!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");

            //PureRx();
            WithTDF();

            Console.ReadKey();
        }

        private static void WithTDF()
        {
            var trns = new TransformBlock<long, byte[]>(async i =>
            {
                using (var http = new HttpClient())
                {
                    var bytes = await http.GetByteArrayAsync(URL);
                    return bytes;
                }
            });

            var xs = Observable.Interval(TimeSpan.FromSeconds(1));

            xs.Subscribe(trns.AsObserver());

            trns.AsObservable().Subscribe(async m =>
            {
                using (var fs = new FileStream($@"Data\{Guid.NewGuid():N}.jpg", FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
                {
                    await fs.WriteAsync(m, 0, m.Length);
                }
            });
        }

        private static void PureRx()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(1))
                                .SelectMany(i => Observable.FromAsync<byte[]>(
                                    async () =>
                                    {
                                        using (var http = new HttpClient())
                                        {
                                            var bytes = await http.GetByteArrayAsync(URL);
                                            return bytes;
                                        }
                                    }));
            xs.Subscribe(async m =>
            {
                using (var fs = new FileStream($@"Data\{Guid.NewGuid():N}.jpg", FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
                {
                    await fs.WriteAsync(m, 0, m.Length);
                }
            });
        }
    }
}
