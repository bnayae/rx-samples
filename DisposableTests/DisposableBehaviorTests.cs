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

#endregion // Using

namespace Bnaya.Samples
{
    [TestClass]
    public class DisposableBehaviorTests
    {
        #region Disposable.Empty

        [TestMethod]
        public void DisposableEmpty_Test()
        {
            // arrange
            var disp = Disposable.Empty;

            // act
            disp.Dispose();
        }

        #endregion // Disposable.Empty

        #region Disposable.Create

        [TestMethod]
        public void DisposableCreate_Test()
        {
            // arrange
            bool disposed = false;
            var disp = Disposable.Create(() => disposed = true);

            // act & verify

            Assert.IsFalse(disposed);
            disp.Dispose();
            Assert.IsTrue(disposed);
        }

        #endregion // Disposable.Create

        #region BooleanDisposable

        [TestMethod]
        public void BooleanDisposable_Test()
        {
            // arrange
            var disp = new BooleanDisposable();

            // act & verify

            Assert.IsFalse(disp.IsDisposed);
            disp.Dispose();
            Assert.IsTrue(disp.IsDisposed);
        }

        #endregion // BooleanDisposable

        #region CancellationDisposable_Cancel_Test

        [TestMethod]
        public void CancellationDisposable_Cancel_Test()
        {
            // arrange
            var cts = new CancellationTokenSource();
            var disp = new CancellationDisposable(cts);

            // act & verify

            Assert.IsFalse(disp.IsDisposed);
            cts.Cancel();
            Assert.IsTrue(disp.IsDisposed);
        }

        #endregion // CancellationDisposable_Cancel_Test

        #region CancellationDisposable_Dispose_Test

        [TestMethod]
        public void CancellationDisposable_Dispose_Test()
        {
            // arrange
            var cts = new CancellationTokenSource();
            var disp = new CancellationDisposable(cts);

            // act & verify

            Assert.IsFalse(cts.Token.IsCancellationRequested);
            disp.Dispose();
            Assert.IsTrue(cts.Token.IsCancellationRequested);
        }

        #endregion // CancellationDisposable_Dispose_Test

        #region RefCountDisposable

        // RefCountDisposable is actually a reference counting disposable

        [TestMethod]
        public void RefCountDisposable_AsFactory_Test()
        {
            // arrange
            const int COUNT = 10;

            var innerDisposable = new BooleanDisposable();
            var dispFactory = new RefCountDisposable(innerDisposable);

            // act 
            var dispodables = from i in Enumerable.Range(0, COUNT)
                                  // will produce a related disposable
                              select dispFactory.GetDisposable();
            dispodables = dispodables.ToArray();

            dispFactory.Dispose(); // Start with single reference

            // verify
            foreach (IDisposable d in dispodables)
            {
                Assert.IsFalse(innerDisposable.IsDisposed);
                Assert.IsFalse(dispFactory.IsDisposed);
                d.Dispose();
            }

            Assert.IsTrue(dispFactory.IsDisposed);
            Assert.IsTrue(innerDisposable.IsDisposed);
        }

        #endregion // RefCountDisposable

        #region CompositeDisposable

        [TestMethod]
        public void CompositeDisposable_DisposeAll_Test()
        {
            // arrange
            var d1 = new BooleanDisposable();
            var d2 = new BooleanDisposable();
            var d3 = new BooleanDisposable();
            var disp = new CompositeDisposable(d1, d2);
            disp.Add(d3);

            // act & verify

            Assert.IsFalse(d1.IsDisposed);
            Assert.IsFalse(d2.IsDisposed);
            Assert.IsFalse(d3.IsDisposed);
            Assert.IsFalse(disp.IsDisposed);

            disp.Dispose(); // dispose all

            Assert.IsTrue(d1.IsDisposed);
            Assert.IsTrue(d2.IsDisposed);
            Assert.IsTrue(d3.IsDisposed);
            Assert.IsTrue(disp.IsDisposed);
        }

        #endregion // CompositeDisposable

        #region SingleAssignmentDisposable

        // see the case of Observable.Create(async ....)
        // kind of eventually disposable
        [TestMethod]
        public async Task SingleAssignmentDisposable_Test()
        {
            // arrange
            var innerDisp =  new BooleanDisposable();
            var disp = new SingleAssignmentDisposable();

            // act & verify
            Assert.IsFalse(disp.IsDisposed);
            disp.Dispose();
            Assert.IsTrue(disp.IsDisposed);
            Assert.IsFalse(innerDisp.IsDisposed);

            var gate = new ManualResetEventSlim(false);
            Task t = Task.Run(() =>
                            {
                                gate.Wait();
                                disp.Disposable = innerDisp;
                            });
            gate.Set();
            await t;
            Assert.IsTrue(disp.IsDisposed);
            Assert.IsTrue(innerDisp.IsDisposed);
        }

        #endregion // SingleAssignmentDisposable

        #region MultipleAssignmentDisposable

        [TestMethod]
        public async Task MultipleAssignmentDisposable_Test()
        {
            // arrange
            var d1 = new BooleanDisposable();
            var d2 = new BooleanDisposable();
            var d3 = new BooleanDisposable();
            // arrange
            var disp = new MultipleAssignmentDisposable();
            disp.Disposable = d1;

            // act & verify
            Assert.IsFalse(disp.IsDisposed);
            Assert.IsFalse(d1.IsDisposed);
            Assert.IsFalse(d2.IsDisposed);
            Assert.IsFalse(d3.IsDisposed);

            disp.Disposable = d2;
            disp.Dispose();

            Assert.IsTrue(disp.IsDisposed);
            Assert.IsFalse(d1.IsDisposed);
            Assert.IsTrue(d2.IsDisposed);
            Assert.IsFalse(d3.IsDisposed);

            var gate = new ManualResetEventSlim(false);
            Task t = Task.Run(() =>
            {
                gate.Wait();
                disp.Disposable = d3;
            });
            gate.Set();
            await t;
            Assert.IsTrue(disp.IsDisposed);
            Assert.IsFalse(d1.IsDisposed);
            Assert.IsTrue(d2.IsDisposed);
            Assert.IsTrue(d3.IsDisposed);
        }

        #endregion // MultipleAssignmentDisposable

        #region SerialDisposable

        [TestMethod]
        public void SerialDisposable_Test()
        {
            // arrange
            var d1 = new BooleanDisposable();
            var d2 = new BooleanDisposable();
            var d3 = new BooleanDisposable();
            var disp = new SerialDisposable();
            disp.Disposable = d1;

            // act & verify
            Assert.IsFalse(d1.IsDisposed);
            Assert.IsFalse(d2.IsDisposed);
            Assert.IsFalse(d3.IsDisposed);
            Assert.IsFalse(disp.IsDisposed);

            disp.Disposable = d2;
            Assert.IsTrue(d1.IsDisposed);
            Assert.IsFalse(d2.IsDisposed);
            Assert.IsFalse(d3.IsDisposed);
            Assert.IsFalse(disp.IsDisposed);

            disp.Dispose();
            Assert.IsTrue(d1.IsDisposed);
            Assert.IsTrue(d2.IsDisposed);
            Assert.IsFalse(d3.IsDisposed);
            Assert.IsTrue(disp.IsDisposed);

            disp.Disposable = d3;
            Assert.IsTrue(d1.IsDisposed);
            Assert.IsTrue(d2.IsDisposed);
            Assert.IsTrue(d3.IsDisposed);
            Assert.IsTrue(disp.IsDisposed);
        }

        #endregion // SerialDisposable

        #region ScheduledDisposable

        [TestMethod]
        public void ScheduledDisposable_Test()
        {
            // arrange
            var scheduler = new TestScheduler();

            var innerDisposable = new BooleanDisposable();
            var disp = new ScheduledDisposable(scheduler, innerDisposable);

            // act 
            disp.Dispose();
            
            // verify
            Assert.IsFalse(disp.IsDisposed);
            scheduler.AdvanceBy(1);
            Assert.IsTrue(disp.IsDisposed);
        }

        #endregion // ScheduledDisposable

        #region ContextDisposable

        [TestMethod]
        public void ContextDisposable1_Test()
        {
            // arrange
            bool isDisposedOnContext = false;
            var context = new CustomSynchronizationContext();

            var innerDisposable = Disposable.Create(() =>
                                      isDisposedOnContext = (SynchronizationContext.Current == context));
            var disp = new ContextDisposable(context, innerDisposable);

            // act 
            disp.Dispose();
            // just blocking until context execution completion 
            context.Wait();

            // verify
            Assert.IsNull(SynchronizationContext.Current);
            Assert.IsTrue(disp.IsDisposed);
            Assert.IsTrue(isDisposedOnContext);
        }

        #endregion // ContextDisposable

        #region CustomSynchronizationContext

        private class CustomSynchronizationContext : SynchronizationContext
        {
            private ConcurrentQueue<Tuple<SendOrPostCallback, object>> _queue = new ConcurrentQueue<Tuple<SendOrPostCallback, object>>();
            private ManualResetEventSlim _sync = new ManualResetEventSlim(true);

            public void RunAsync()
            {
                if (!_sync.Wait(1))
                    return;

                _sync.Reset();

                Action run = RunContext;
                Task.Run(run);
            }

            private void RunContext()
            {
                SynchronizationContext.SetSynchronizationContext(this);
                Tuple<SendOrPostCallback, object> tuple;
                while (_queue.TryDequeue(out tuple))
                {
                    tuple.Item1(tuple.Item2);
                }
                _sync.Set();
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                _queue.Enqueue(Tuple.Create(d, state));
                RunAsync();
                _sync.Wait();
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                _queue.Enqueue(Tuple.Create(d, state));
                RunAsync();
            }

            public void Wait()
            {
                _sync.Wait();
            }
        }

        #endregion // CustomSynchronizationContext
    }
}
