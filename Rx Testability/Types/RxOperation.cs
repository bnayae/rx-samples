using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnaya.Samples
{
    public class RxOperation
    {
        public const int LIMIT = 10;

        private readonly IScheduler _scheduler;
        public RxOperation(IScheduler scheduler = null)
        {
            _scheduler = scheduler ?? Scheduler.Default;
        }

        public IObservable<string> TenStars(int limit = LIMIT)
        {
            var source = Observable.Interval(TimeSpan.FromMinutes(1), _scheduler);
            var stars = from item in source
                        select new string('*', (int)item + 1);
            return stars.Take(limit);
        }
    }
}
