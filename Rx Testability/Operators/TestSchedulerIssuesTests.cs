#region Using

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Reactive.Testing;
using System.Threading.Tasks;
using System.Reactive.Concurrency;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Reactive;
using System.Reactive.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

#endregion // Using

namespace Bnaya.Samples
{
    [TestClass]
    public class TestSchedulerIssuesTests
    {
        private TestScheduler _scd;

        #region Setup

        [TestInitialize]
        public void Setup()
        {
            _scd = new TestScheduler();
        }

        #endregion // Setup

        [TestMethod]
        public async Task SingleThreadingScheduler_Cause_Deadlock_Tests()
        {
            //var dataStream = uris.FromTdf(() => block);

            //var xs = Observable.Interval(TimeSpan.FromMinutes(1), _scd)
            //    .Take(10);
            var xs = Observable.FromAsync(AwaiableCall);

            //var observer = _scd.CreateObserver<long>();
            var observer = new MockObserver<long>();
            xs.Subscribe(observer);

            Trace.WriteLine("**" + Thread.CurrentThread.ManagedThreadId);
            _scd.AdvanceBy(100);
            Trace.WriteLine("------------------");
            await Task.Yield();
            Trace.WriteLine("------------------");
            Assert.AreEqual(1, observer.Messages.Length);
        }

        public async Task<long> AwaiableCall()
        {
            Trace.WriteLine("Before: Thread =" + Thread.CurrentThread.ManagedThreadId);
            await Observable.Timer(TimeSpan.FromTicks(10), _scd);
            Trace.WriteLine("After: Thread = " + Thread.CurrentThread.ManagedThreadId);
            return _scd.Clock;
        }

        public Task<long> WaitHandledCall(long id)
        {
            var sync = new ManualResetEvent(false);
            Observable.Timer(TimeSpan.FromTicks(10), _scd)
                .Finally(() => sync.Set())
                .Subscribe();
            if (!sync.WaitOne(TimeSpan.FromSeconds(1)))
                throw new TimeoutException("Deadlock");
            //Trace.WriteLine("## End " + id);
            return Task.FromResult(_scd.Clock);
        }
    }

    public class MockObserver<T> : IObserver<T>
    {
        private readonly ConcurrentQueue<Notification<T>> _queue = new ConcurrentQueue<Notification<T>>();

        public void OnCompleted()
        {
            Trace.WriteLine("Completed on Thread " + Thread.CurrentThread.ManagedThreadId);
            _queue.Enqueue(Notification.CreateOnCompleted<T>());
        }

        public void OnError(Exception error)
        {
            Trace.WriteLine("Error:" + error.Message + " on Thread " + Thread.CurrentThread.ManagedThreadId);
           _queue.Enqueue(Notification.CreateOnError<T>(error));
        }

        public void OnNext(T value)
        {
            Trace.WriteLine("[" + value + "] on Thread " + Thread.CurrentThread.ManagedThreadId);
            _queue.Enqueue(Notification.CreateOnNext(value));
        }

        public Notification<T>[] Messages { get { return _queue.ToArray(); } }


 
    }

}

public static class Ex
{
    public static IObservable<TResult> FromTdf<T, TResult>(
        this IObservable<T> source, 
        IPropagatorBlock<T, TResult> block)
    {
        return Observable.Defer<TResult>(() =>
            {
                source.Subscribe(block.AsObserver());
                return block.AsObservable();
            });
    }

    public static IObservable<TResult> FromTdf<T, TResult>(
        this IObservable<T> source, 
        Func<IPropagatorBlock<T, TResult>> blockFactory)
    {
        return Observable.Defer<TResult>(() =>
            {
                var block = blockFactory();
                source.Subscribe(block.AsObserver());
                return block.AsObservable();
            });
    }
}

/*
     public virtual IObservable<TResult> FromAsync<TResult>(Func<Task<TResult>> functionAsync)
    {
        return this.Defer<TResult>(() => this.StartAsync<TResult>(functionAsync));
    }
public virtual IObservable<TSource> StartAsync<TSource>(Func<Task<TSource>> functionAsync)
{
    Task<TSource> task = null;
    try
    {
        task = functionAsync();
    }
    catch (Exception exception)
    {
        return this.Throw<TSource>(exception);
    }
    return task.ToObservable<TSource>();
}
public static IObservable<TResult> ToObservable<TResult>(this Task<TResult> task)
{
    if (task == null)
    {
        throw new ArgumentNullException("task");
    }
    AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
    if (task.IsCompleted)
    {
        TaskObservableExtensions.ToObservableDone<TResult>(task, asyncSubject);
    }
    else
    {
        TaskObservableExtensions.ToObservableSlow<TResult>(task, asyncSubject);
    }
    return asyncSubject.AsObservable<TResult>();
}
private static void ToObservableSlow<TResult>(Task<TResult> task, AsyncSubject<TResult> source)
{
    task.ContinueWith(delegate(Task<TResult> t)
    {
        TaskObservableExtensions.ToObservableDone<TResult>(t, source);
    });
}

private static void ToObservableDone<TResult>(Task<TResult> task, AsyncSubject<TResult> source)
{
    switch (task.Status)
    {
        case TaskStatus.RanToCompletion:
            source.OnNext(task.Result);
            source.OnCompleted();
            return;
        case TaskStatus.Canceled:
            source.OnError(new TaskCanceledException(task));
            return;
        case TaskStatus.Faulted:
            source.OnError(task.Exception.InnerException);
            return;
        default:
            return;
    }
}
 */
