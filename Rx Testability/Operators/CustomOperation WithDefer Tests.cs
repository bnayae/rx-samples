#region Using

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using System.Threading.Tasks;
using System.Reactive.Concurrency;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Reactive;
using System.Reactive.Subjects;

#endregion // Using

namespace Bnaya.Samples
{
    using static Microsoft.Reactive.Testing.ReactiveTest;

    [TestClass]
    public class CustomOperation_WithDefer_Tests
    {
        #region NonCachedService_Tests

        [TestMethod]
        public void Pause_Tests()
        {
            // arrange
            var scd = new TestScheduler();
            var source = scd.CreateColdObservable(
                        OnNext(10, "A"),
                        OnNext(20, "B"),
                        OnNext(30, "C"),
                        OnNext(40, "D"),
                        OnNext(50, "E"),
                        OnCompleted<string>(60));
            var trigger = scd.CreateColdObservable(
                        OnNext(25, true),
                        OnNext(26, true),
                        OnNext(45, false));

            // act
            var observer =
                scd.Start(() => source.Pause(trigger, scd), 0, 0, 1000);

            // validate
            observer.Messages.AssertEqual(
                        OnNext(11, "A"),
                        OnNext(21, "B"),
                        OnNext(51, "E"),
                        OnCompleted<string>(61));
        }

        #endregion // NonCachedService_Tests
    }

    public static class PauseExtentions
    {
        public static IObservable<T> Pause<T>(
            this IObservable<T> source,
            IObservable<bool> pauseTrigger,
            IScheduler scheduler = null)
        {
            scheduler = scheduler ?? Scheduler.Default;
            var result = Observable.Defer<T>(() =>
            {
                var state = new BehaviorSubject<bool>(false);
                pauseTrigger.DistinctUntilChanged()
                            .Subscribe(state);

                var output = source.Where(_ => !state.Value);
                return output;
            });
            return result;
        }
    }
}
