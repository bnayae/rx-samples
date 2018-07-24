using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnaya.Samples
{
    public class MyComponent
    {
        private readonly IScheduler _scheduler;

        public MyComponent(IScheduler scheduler)
        {
            _scheduler = scheduler;
            Data = Observable.Interval(TimeSpan.FromSeconds(1), scheduler)
                            .Where(v => v % 2 == 0);
        }

        public IObservable<long> Data { get;  }
    }
}
