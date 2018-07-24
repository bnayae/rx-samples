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
    /// <summary>
    /// LongRunning, Periodic, Stopwatch
    /// </summary>
    [TestClass]
    public class SchedulerVariationTest
    {
        private SimpleScheduler _scheduler;

        #region Setup

        [TestInitialize]
        public void Setup()
        {
            _scheduler = new SimpleScheduler();
        }

        #endregion // Setup

        #region Retun_ShouldUse_BasicScheduler_Test

        [TestMethod]
        public void Retun_ShouldUse_BasicScheduler_Test()
        {
            // arrange
            var xs = Observable.Return(1, _scheduler);

            // act
            xs.Wait();

            // verify
            Assert.IsFalse(_scheduler.IsTargetLongRunning);
            Assert.IsFalse(_scheduler.IsTargetPeriodic);
        }

        #endregion // Retun_ShouldUse_BasicScheduler_Test

        #region Range_ShouldUse_LongRuningScheduler_Test

        [TestMethod]
        public void Range_ShouldUse_LongRuningScheduler_Test()
        {
            // arrange
            var xs = Observable.Range(1, 100, _scheduler);

            // act
            xs.Wait();

            // verify
            Assert.IsTrue(_scheduler.IsTargetLongRunning);
            Assert.IsFalse(_scheduler.IsTargetPeriodic);
        }

        #endregion // Range_ShouldUse_LongRuningScheduler_Test

        #region Interval_ShouldUse_PeriodicScheduler_Test

        [TestMethod]
        public void Interval_ShouldUse_PeriodicScheduler_Test()
        {
            // arrange
            var xs = Observable.Interval(TimeSpan.FromTicks(1), _scheduler)
                .Take(10);

            // act
            xs.Wait();

            // verify
            Assert.IsFalse(_scheduler.IsTargetLongRunning);
            Assert.IsTrue(_scheduler.IsTargetPeriodic);
        }

        #endregion // Interval_ShouldUse_PeriodicScheduler_Test

        #region Repeat_ShouldUse_LongRunningScheduler_Test

        [TestMethod]
        public void Repeat_ShouldUse_LongRunningScheduler_Test()
        {
            // arrange
            var xs = Observable.Repeat(1, 100, _scheduler);

            // act
            xs.Wait();

            // verify
            Assert.IsTrue(_scheduler.IsTargetLongRunning);
            Assert.IsFalse(_scheduler.IsTargetPeriodic);
        }

        #endregion // Repeat_ShouldUse_LongRunningScheduler_Test

        #region Timestamp_ShouldUse_BasicScheduler_Test

        [TestMethod]
        public void Timestamp_ShouldUse_BasicScheduler_Test()
        {
            // arrange
            var xs = Observable.Return(1).Timestamp(_scheduler);

            // act
            xs.Wait();

            // verify
            Assert.IsFalse(_scheduler.IsTargetLongRunning);
            Assert.IsFalse(_scheduler.IsTargetPeriodic);
        }

        #endregion // Timestamp_ShouldUse_BasicScheduler_Test
    }
}
