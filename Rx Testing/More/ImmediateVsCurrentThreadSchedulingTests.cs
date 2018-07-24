#region Using

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;
using System.Reactive.Threading;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using System.Diagnostics;
using System.Reactive.Subjects;

#endregion // Using

namespace Bnaya.Samples
{
    [TestClass]
    public class ImmediateVsCurrentThreadSchedulingTests
    {
        //private static readonly long ONE_MINUTE_TICKS = TimeSpan.FromMinutes(1).Ticks;
        //private readonly TestScheduler _scheduler = new TestScheduler();

        #region ImmediateScheduler_Test

        [TestMethod]
        public void ImmediateScheduler_Test()
        {
            // arrange
            const string BEGIN = "begin";
            const string END = "end";

            string beginFormat = "Level {0} begin";
            string endFormat = "Level {0} end";
            string atomicFormat = "Level {0}";

            int startThreadId = Thread.CurrentThread.ManagedThreadId;
            var executionOrder = new List<Tuple<int, int, string>>(); // level, thread id, begin / end 

            var scheduler = ImmediateScheduler.Instance;

            // act
            scheduler.Schedule(() =>
             {
                 Trace.WriteLine(string.Format(beginFormat, 1));
                 executionOrder.Add(Tuple.Create(1, Thread.CurrentThread.ManagedThreadId, BEGIN));

                 scheduler.Schedule(() =>
                   {
                       // immediate (direct invocation) will execute it before level 1 ending 
                       // CurrentThread (queue) will execute it after level 1 ending 
                       //  (on completion of level 1 scheduling it will pop the queue
                       //   it is also the case for EventLoopScheduler)
                       Trace.WriteLine(string.Format(beginFormat, 2));
                       executionOrder.Add(Tuple.Create(2, Thread.CurrentThread.ManagedThreadId, BEGIN));

                       scheduler.Schedule(() =>
                         {
                             // immediate (direct invocation) will execute it before level 2 ending 
                             // CurrentThread (queue) will execute it after level 2 ending 
                             //  (on completion of level 2 scheduling it will pop the queue
                             //   it is also the case for EventLoopScheduler)
                             Trace.WriteLine(string.Format(atomicFormat, 3));
                             executionOrder.Add(Tuple.Create(3, Thread.CurrentThread.ManagedThreadId, string.Empty));
                         });

                       Trace.WriteLine(string.Format(endFormat, 2));
                       executionOrder.Add(Tuple.Create(2, Thread.CurrentThread.ManagedThreadId, END));
                   });

                 Trace.WriteLine(string.Format(endFormat, 1));
                 executionOrder.Add(Tuple.Create(1, Thread.CurrentThread.ManagedThreadId, END));
             });

            // verify
            Assert.AreEqual(Tuple.Create(1, startThreadId, BEGIN), executionOrder[0]);
            Assert.AreEqual(Tuple.Create(2, startThreadId, BEGIN), executionOrder[1]);
            Assert.AreEqual(Tuple.Create(3, startThreadId, string.Empty), executionOrder[2]);
            Assert.AreEqual(Tuple.Create(2, startThreadId, END), executionOrder[3]);
            Assert.AreEqual(Tuple.Create(1, startThreadId, END), executionOrder[4]);
        }

        #endregion // ImmediateScheduler_Test

        #region CurrentThreadScheduler_Test

        [TestMethod]
        public void CurrentThreadScheduler_Test()
        {
            // arrange
            const string BEGIN = "begin";
            const string END = "end";

            string beginFormat = "Level {0} begin";
            string endFormat = "Level {0} end";
            string atomicFormat = "Level {0}";

            int startThreadId = Thread.CurrentThread.ManagedThreadId;
            var executionOrder = new List<Tuple<int, int, string>>(); // level, thread id, begin / end 

            var scheduler = CurrentThreadScheduler.Instance;

            // act
            scheduler.Schedule(() =>
             {
                 Trace.WriteLine(string.Format(beginFormat, 1));
                 executionOrder.Add(Tuple.Create(1, Thread.CurrentThread.ManagedThreadId, BEGIN));

                 scheduler.Schedule(() =>
                   {
                       // CurrentThread (queue) will execute it after level 1 ending 
                       //  (on completion of level 1 scheduling it will pop the queue
                       //   it is also the case for EventLoopScheduler)
                       // immediate (direct invocation) will execute it before level 1 ending 
                       Trace.WriteLine(string.Format(beginFormat, 2));
                       executionOrder.Add(Tuple.Create(2, Thread.CurrentThread.ManagedThreadId, BEGIN));

                       scheduler.Schedule(() =>
                         {
                             // CurrentThread (queue) will execute it after level 2 ending 
                             //  (on completion of level 2 scheduling it will pop the queue
                             //   it is also the case for EventLoopScheduler)
                             // immediate (direct invocation) will execute it before level 2 ending 
                             Trace.WriteLine(string.Format(atomicFormat, 3));
                             executionOrder.Add(Tuple.Create(3, Thread.CurrentThread.ManagedThreadId, string.Empty));
                         });

                       Trace.WriteLine(string.Format(endFormat, 2));
                       executionOrder.Add(Tuple.Create(2, Thread.CurrentThread.ManagedThreadId, END));
                   });

                 Trace.WriteLine(string.Format(endFormat, 1));
                 executionOrder.Add(Tuple.Create(1, Thread.CurrentThread.ManagedThreadId, END));
             });

            // verify
            Assert.AreEqual(Tuple.Create(1, startThreadId, BEGIN), executionOrder[0]);
            Assert.AreEqual(Tuple.Create(1, startThreadId, END), executionOrder[1]);
            Assert.AreEqual(Tuple.Create(2, startThreadId, BEGIN), executionOrder[2]);
            Assert.AreEqual(Tuple.Create(2, startThreadId, END), executionOrder[3]);
            Assert.AreEqual(Tuple.Create(3, startThreadId, string.Empty), executionOrder[4]);
        }

        #endregion // CurrentThreadScheduler_Test

        #region CurrentThreadScheduler_Scenario_OrderAsExpected_Test

        [TestMethod]
        public void CurrentThreadScheduler_Scenario_OrderAsExpected_Test()
        {
            // arrange
            var expected = new List<int>();
            IScheduler scheduler = CurrentThreadScheduler.Instance;
            List<int> results = new List<int>();
            var source = Observable.Return(1, scheduler)
                .Concat(Observable.Never<int>()); // avoid completion side effects

            // act
            var hot = Scenario(source,
                expected, results, scheduler,
                xs => xs.Publish()) as IConnectableObservable<int>;
            hot.Connect();

            // verify
            ValidateCorrectOrder(expected, results);
        }

        #endregion // CurrentThreadScheduler_Scenario_OrderAsExpected_Test

        #region ImmediateScheduler_Scenario_NotExpectedOrder_Test

        [TestMethod]
        public void ImmediateScheduler_Scenario_NotExpectedOrder_Test()
        {
            // arrange
            var expected = new List<int>();
            IScheduler scheduler = ImmediateScheduler.Instance;
            List<int> results = new List<int>();
            var source = Observable.Return(1, scheduler)
                .Concat(Observable.Never<int>()); // avoid completion side effects

            // act
            var hot = Scenario(source,
                expected, results, scheduler, 
                xs => xs.Publish()) as IConnectableObservable<int>;
            hot.Connect();

            // verify
            ValidateIncorrectOrder(expected, results);
        }

        #endregion // ImmediateScheduler_Scenario_NotExpectedOrder_Test

        #region CurrentThreadScheduler_Scenario_RefCount_OrderAsExpected_Test

        [TestMethod]
        public void CurrentThreadScheduler_Scenario_RefCount_OrderAsExpected_Test()
        {
            // arrange
            var expected = new List<int>();
            List<int> results = new List<int>();
            IScheduler scheduler = CurrentThreadScheduler.Instance;
            var source = Observable.Range(1, 3, scheduler); 
                //.Concat(Observable.Never<int>()); // avoid completion side effects
           
            // act
            Scenario(source,
                expected, results, scheduler,
                xs => xs.Publish().RefCount());

            // verify
            ValidateCorrectOrder(expected, results);
        }

        #endregion // CurrentThreadScheduler_Scenario_RefCount_OrderAsExpected_Test

        #region ImmediateScheduler_Scenario_RefCount_NotExpectedOrder_Test

        [TestMethod]
        public void ImmediateScheduler_Scenario_RefCount_NotExpectedOrder_Test()
        {
            // arrange
            var expected = new List<int>();
            List<int> results = new List<int>();
            IScheduler scheduler = ImmediateScheduler.Instance;
            var source = Observable.Range(1, 3, scheduler); // WILL CAUSE RE_SUBSCRIPTION (subscribe after RefCount completion will retrigger the underline cold observable)
                    //.Concat(Observable.Never<int>()); // avoid completion side effects

            // act
            Scenario(source,
                expected, results, scheduler,
                xs => xs.Publish().RefCount());

            // verify
            ValidateIncorrectOrder(expected, results);
        }

        #endregion // ImmediateScheduler_Scenario_RefCount_NotExpectedOrder_Test

        #region NewThreadScheduler_Test

        [TestMethod]
        public void NewThreadScheduler_Test()
        {
            // arrange
            const string BEGIN = "begin";
            const string END = "end";

            string beginFormat = "Level {0} begin";
            string endFormat = "Level {0} end";
            string atomicFormat = "Level {0}";

            int startThreadId = Thread.CurrentThread.ManagedThreadId;
            var executionOrder = new List<Tuple<int, string>>(); // level, begin / end 
            var scheduledThreadIds = new List<int>();

            var scheduler = NewThreadScheduler.Default;

            // act
            using (var cd = new CountdownEvent(3))
            {
                scheduler.Schedule(() =>
                 {
                     // will execute on a new thread
                     Trace.WriteLine(string.Format(beginFormat, 1));
                     executionOrder.Add(Tuple.Create(1, BEGIN));
                     scheduledThreadIds.Add(Thread.CurrentThread.ManagedThreadId);

                     scheduler.Schedule(() =>
                       {
                           // will execute on a new thread
                           Trace.WriteLine(string.Format(beginFormat, 2));
                           executionOrder.Add(Tuple.Create(2, BEGIN));
                           scheduledThreadIds.Add(Thread.CurrentThread.ManagedThreadId);

                           scheduler.Schedule(() =>
                             {
                                 // will execute on a new thread
                                 Trace.WriteLine(string.Format(atomicFormat, 3));
                                 executionOrder.Add(Tuple.Create(3, string.Empty));
                                 scheduledThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                                 cd.Signal();
                             });

                           Trace.WriteLine(string.Format(endFormat, 2));
                           executionOrder.Add(Tuple.Create(2, END));
                           scheduledThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                           cd.Signal();
                       });

                     Trace.WriteLine(string.Format(endFormat, 1));
                     executionOrder.Add(Tuple.Create(1, END));
                     scheduledThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                     cd.Signal();
                 });
                scheduler.Schedule(() =>
                    {
                        // will execute on a new thread
                        Trace.WriteLine(string.Format(beginFormat, 1));
                        executionOrder.Add(Tuple.Create(4, string.Empty));
                        scheduledThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                    });
                cd.Wait();
            }

            // verify
            Assert.AreEqual(4, scheduledThreadIds.Distinct().Count());
            Assert.IsFalse(scheduledThreadIds.Any(id => startThreadId == id));
        }

        #endregion // NewThreadScheduler_Test

        #region EventLoopScheduler_Test

        [TestMethod]
        public void EventLoopScheduler_Test()
        {
            // arrange
            const string BEGIN = "begin";
            const string END = "end";

            string beginFormat = "Level {0} begin";
            string endFormat = "Level {0} end";
            string atomicFormat = "Level {0}";

            int startThreadId = Thread.CurrentThread.ManagedThreadId;
            var executionOrder = new List<Tuple<int, string>>(); // level, begin / end 
            var scheduledThreadIds = new List<int>();

            var scheduler = new EventLoopScheduler();

            // act
            using (var cd = new CountdownEvent(3))
            {
                scheduler.Schedule(() =>
                 {
                     Trace.WriteLine(string.Format(beginFormat, 1));
                     executionOrder.Add(Tuple.Create(1, BEGIN));
                     scheduledThreadIds.Add(Thread.CurrentThread.ManagedThreadId);

                     scheduler.Schedule(() =>
                       {
                           Trace.WriteLine(string.Format(beginFormat, 2));
                           executionOrder.Add(Tuple.Create(2, BEGIN));
                           scheduledThreadIds.Add(Thread.CurrentThread.ManagedThreadId);

                           scheduler.Schedule(() =>
                             {
                                 Trace.WriteLine(string.Format(atomicFormat, 3));
                                 executionOrder.Add(Tuple.Create(3, string.Empty));
                                 scheduledThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                                 cd.Signal();
                             });

                           Trace.WriteLine(string.Format(endFormat, 2));
                           executionOrder.Add(Tuple.Create(2, END));
                           scheduledThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                           cd.Signal();
                       });

                     Trace.WriteLine(string.Format(endFormat, 1));
                     executionOrder.Add(Tuple.Create(1, END));
                     scheduledThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                     cd.Signal();
                 });
                cd.Wait();
            }

            // verify
            Assert.AreEqual(1, scheduledThreadIds.Distinct().Count());
            Assert.IsFalse(scheduledThreadIds.Any(id => startThreadId == id));

            Assert.AreEqual(Tuple.Create(1, BEGIN), executionOrder[0]);
            Assert.AreEqual(Tuple.Create(1, END), executionOrder[1]);
            Assert.AreEqual(Tuple.Create(2, BEGIN), executionOrder[2]);
            Assert.AreEqual(Tuple.Create(2, END), executionOrder[3]);
            Assert.AreEqual(Tuple.Create(3, string.Empty), executionOrder[4]);
        }

        #endregion // EventLoopScheduler_Test

        #region Scenario

        /// <summary>
        /// Scenario for immediate vs. current thread scheduler differences.
        /// </summary>
        /// <param name="source">the source stream</param>
        /// <param name="expected">The expected.</param>
        /// <param name="results">The results list.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <param name="createHotStream">The create hot stream.</param>
        /// <returns></returns>
        /// <remarks>
        /// return -o------------------------
        /// Window --------------------------
        ///         o-|
        /// Last    --o-|
        /// </remarks>
        private static IObservable<int> Scenario(
            IObservable<int> source,
            List<int> expected, 
            List<int> results,
            IScheduler scheduler,
            Func<IObservable<int>, IObservable<int>> createHotStream)
        {
            int factor = 0;
            source = source.Do(v =>
                {
                    factor = (v - 1) * 6;
                    expected.Add(1 + factor);
                }); // second value

            source = source.SubscriptionMonitor(() => expected.Add(0));
            var hot = createHotStream(source); // Hot stream

            var windows = hot.Window(
                            hot.Do(v => expected.Add(2 + factor)),               // Opener window trigger (2nd subscription)
                            openingValue => Observable.Empty<Unit>(scheduler)
                                .Do(v => { }, () => expected.Add(4 + factor)));  // close window trigger each item 

            windows = windows.Do(
                v => expected.Add(3 + factor));// IMMEDIATE WILL REORDER THIS ONE !!!, windows creation (triggered by the opening)

            var aggregation = from win in windows
                              from item in win.LastAsync()
                                             .Do(v => results.Add(v))
                                             .Do(v => expected.Add(5 + factor), // complete aggregation (triggered by the closing)
                                                () => expected.Add(6 + factor)) // close the window
                              select item;

            aggregation.Subscribe();
            return hot;
        }

        #endregion // Scenario

        #region ValidateCorrectOrder

        private static void ValidateCorrectOrder(List<int> expected, List<int> results)
        {
            bool isOrdered = true;
            Trace.Write(string.Format("Execution Order: {0}, ", expected[0]));
            for (int i = 1; i < expected.Count; i++)
            {
                Trace.Write(string.Format("{0}, ", expected[i]));
                if (expected[i - 1] != expected[i] - 1)
                    isOrdered = false;
            }

            // as expected
            Assert.AreNotEqual(0, results.Count);
            for (int i = 0; i < results.Count; i++)
            {
                Assert.AreEqual(i + 1, results[i]);
            }
            Assert.IsTrue(isOrdered);
        }

        #endregion // ValidateCorrectOrder

        #region ValidateIncorrectOrder

        private static void ValidateIncorrectOrder(List<int> expected, List<int> results)
        {
            bool isOrdered = true;
            Trace.Write(string.Format("Execution Order: {0}, ", expected[0]));
            for (int i = 1; i < expected.Count; i++)
            {
                Trace.Write(string.Format("{0},", expected[i]));
                if (expected[i - 1] != expected[i] - 1)
                    isOrdered = false;
            }

            // not expected
            Assert.AreEqual(0, results.Count);
            Assert.IsFalse(isOrdered); // THE WINDOWS CREATION SCHEDULED AFTER THE CLOSING EVENT
        }

        #endregion // ValidateIncorrectOrder
    }
}
