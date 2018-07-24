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
    /// Handling exception at the schedule level
    /// </summary>
    [TestClass]
    public class SchedulerExceptionHandlingTest
    {
        private TestScheduler _testScheduler;
        private IScheduler _scheduler;
        private List<Exception> _exceptions = new List<Exception>();
        private ITestableObserver<int> _observer;
        private AutoResetEvent _sync;
        private bool _setSyncEventOnCatch = true;

        #region Setup

        [TestInitialize]
        public void Setup()
        {
            _exceptions.Clear();

            _testScheduler = new TestScheduler();
            _observer = _testScheduler.CreateObserver<int>();

            _scheduler = DefaultScheduler.Instance 
                .Catch<Exception>(ex =>
                    {
                        var exc = new ExecutionEngineException("", ex);
                        _exceptions.Add(exc);
                        if(_setSyncEventOnCatch)
                            _sync.Set();
                        return true;
                    })
                .Catch<ArgumentException>(ex =>
                    {
                        _exceptions.Add(ex);
                        if(_setSyncEventOnCatch)
                            _sync.Set();
                        return true;
                    });

            _sync = new AutoResetEvent(false);
        }

        #endregion // Setup

        #region Scheduler_Swallow_Exceptions_Test

        [TestMethod]
        public void Scheduler_Swallow_Exceptions_Test()
        {
            // arrange

            // act
            _scheduler.Schedule(() => { throw new ArgumentException(); });
            _scheduler.Schedule(() => { throw new NotImplementedException(); });

            _sync.WaitOne();
            _sync.WaitOne();

            // verify
            Assert.AreEqual(2, _exceptions.Count);
            Assert.IsTrue(_exceptions.Any(e => e is ArgumentException));
            Assert.IsTrue(_exceptions.Any(e => e is ExecutionEngineException));
            Assert.AreEqual(1, _exceptions.Count(e => e.InnerException is NotImplementedException));
        }

        #endregion // Scheduler_Swallow_Exceptions_Test

        #region Scheduler_Do_FailToCatchExceptions_Test

        [TestMethod]
        public void Scheduler_Do_FailToCatchExceptions_Test()
        {
            // arrange
            bool catchError = true;
            var xs = Observable.Range(1, 3, _scheduler)
                .Do(v => 
                    {
                        switch (v)
	                    {
		                    case 1:
                                throw new ArgumentException();
		                    case 2:
                                throw new NotImplementedException();
	                    }
                    });

            // act
            try
            {
                xs.Wait();
            }
            catch (Exception)
            {
                catchError = false; 
            }

            // verify
            Assert.IsFalse(catchError);
        }

        #endregion // Scheduler_Do_FailToCatchExceptions_Test

        #region Scheduler_Do_RouteException_ToOnError_Test

        [TestMethod]
        public void Scheduler_Do_RouteException_ToOnError_Test()
        {
            // arrange
            var xs = Observable.Return(1, _scheduler)
                .Do(v => {throw new ArgumentException();});

            // act
            xs = xs.Publish().RefCount();
            xs.Subscribe(_observer);
            xs.Catch(Observable.Return(-1)).Wait();

            // verify
            Assert.AreEqual(1, _observer.Messages.Count);
            Assert.AreEqual(NotificationKind.OnError, _observer.Messages[0].Value.Kind);
        }

        #endregion // Scheduler_Do_RouteException_ToOnError_Test

        #region Scheduler_Swallow_BuggyObserver_Exceptions_Test

        [TestMethod]
        public void Scheduler_Swallow_BuggyObserver_Exceptions_Test()
        {
            // arrange
            _setSyncEventOnCatch = false;
            bool hasValue = false;
            bool isComplete = false;

            var buggyObserver = new BuggyObserver();
            var xs = Observable.Return(1, _scheduler);

            // act
            xs = xs.Publish().RefCount();
            xs.Subscribe(buggyObserver);
            xs.Do(v => Trace.WriteLine(v), ex => Trace.WriteLine(ex), () => Trace.WriteLine("Complete"))
                .Subscribe(
                    v => hasValue = true,
                    ex => _sync.Set(),
                    () => { isComplete = true; _sync.Set(); });

            _sync.WaitOne();

            // verify
            Assert.IsTrue(hasValue);
            Assert.IsTrue(isComplete);
        }

        #endregion // Scheduler_Swallow_BuggyObserver_Exceptions_Test
    }
}
