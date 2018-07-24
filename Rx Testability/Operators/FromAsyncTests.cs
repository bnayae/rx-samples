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
using System.Runtime.Caching;

#endregion // Using

namespace Bnaya.Samples
{
    [TestClass]
    public class FromAsyncTests
    {
        #region NonCachedService_Tests

        [TestMethod]
        public void NCachedService_Tests()
        {
            var scd = new TestScheduler();
            var cache = new Proxy(scd);

            var xs = Observable.Interval(TimeSpan.FromMinutes(1), scd)
                .Take(10);
            var ys = from item in xs
                     from data in Observable.FromAsync(() => cache.RemoteCall(item % 3))
                     select data;

            var observer = scd.CreateObserver<Data>();
            ys.Subscribe(observer);

            var sec = TimeSpan.FromMinutes(1).Ticks;
            for (int i = 0; i < 10; i++) // _scd.AdvanceBy(sec * 10) don't do the job (probably some race condition)
            {
                scd.AdvanceBy(sec);
                scd.AdvanceBy(1);
            }
            Assert.AreEqual(8, observer.Messages.Count, "2 messages is still running");
            scd.AdvanceBy(sec);
            Assert.AreEqual(9, observer.Messages.Count, "1 message is still running");
            scd.AdvanceBy(sec);
            Assert.AreEqual(10, observer.Messages.Count, "just befor completion");
            scd.AdvanceBy(1);
            Assert.AreEqual(11, observer.Messages.Count, "completed");

            var nextMessages = from item in observer.Messages
                               let nfy = item.Value
                               where nfy.Kind == NotificationKind.OnNext
                               select nfy.Value;

            Assert.IsTrue(nextMessages.All(r => r.IsCached == false));
        }

        #endregion // NonCachedService_Tests

        [TestMethod]
        public void CachedService_Tests()
        {
            var scd = new TestScheduler();
            var cache = new Proxy(scd);

            var xs = Observable.Interval(TimeSpan.FromSeconds(1), scd)
                .Take(10);
            var ys = from item in xs
                     from data in Observable.FromAsync(() => cache.CacheableoRemoteCall(item % 3))
                     select data;

            var observer = scd.CreateObserver<Data>();
            ys.Subscribe(observer);

            var sec = TimeSpan.FromSeconds(1).Ticks;
            scd.AdvanceBy(sec * 3);

            Assert.AreEqual(3, observer.Messages.Count, "2 messages is still running");
            Assert.IsTrue(observer.Messages.All(r => r.Value.Value.IsCached == false));

            scd.AdvanceBy(sec * 10);
            Assert.AreEqual(10, observer.Messages.Count, "just befor completion");
            Assert.IsTrue(observer.Messages.Skip(3).All(r => r.Value.Value.IsCached == true));
            scd.AdvanceBy(1);
            Assert.AreEqual(11, observer.Messages.Count, "completed");
        }


        public class Proxy
        {
            private IScheduler _scd;
            //private readonly ConcurrentDictionary<long, Task<Data>> _cache = new ConcurrentDictionary<long, Task<Data>>();
            private readonly MemoryCache _cache = MemoryCache.Default;
            public Proxy(IScheduler scd)
            {
                _scd = scd;
            }

            public Task<Data> CacheableoRemoteCall(long val)
            {

                string key = val.ToString();
                Task<Data> resut = _cache[key] as Task<Data>;
                if (resut != null)
                {
                    resut.Result.IsCached = true;
                    return resut;
                }

                Task<Data> result = RemoteCall(val);
                var policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(1) };

                _cache.Add(key, result, policy);
                return result;
            }

            public async Task<Data> RemoteCallAsync(long id)
            {
                await Observable.Timer(TimeSpan.FromMinutes(2), _scd);
                //Trace.WriteLine("## End " + id);
                string value = new string('*', (int)id + 1);
                var data = new Data { Value = value };
                return data;
            }

            public Task<Data> RemoteCall(long id)
            {
                var sync = new ManualResetEvent(false);
                Observable.Timer(TimeSpan.FromMinutes(2), _scd).Finally(() => sync.Set()).Subscribe();
                sync.WaitOne();
                //Trace.WriteLine("## End " + id);
                string value = new string('*', (int)id + 1);
                var data = new Data { Value = value };
                return Task.FromResult(data);
            }
        }
        public class Data
        {
            public string Value { get; set; }
            public bool IsCached { get; set; }
        }
    }
}
