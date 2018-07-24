#region Using

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Threading.Tasks;
using System.Reactive.Disposables;
using Microsoft.Reactive.Testing;
using System.Reactive;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

#endregion // Using

namespace Bnaya.Samples
{
    // ENABLE DIRECT ACCESS TO STATIC METHOD
    using static Microsoft.Reactive.Testing.ReactiveTest;

    [TestClass]
    public class UnitTest1
    {
        private static readonly long ONE_MINUTE_TICKS = TimeSpan.FromMinutes(1).Ticks;
        private readonly TestScheduler _scheduler = new TestScheduler();
        private RxOperation _instance;

        [TestInitialize]
        public void Setup()
        {
            _instance = new RxOperation(_scheduler);
        }

        #region PresetAndPostset_Test

        [TestMethod]
        public void PresetAndPostset_Test()
        {
            // arrange
            var hotSource = _scheduler.CreateHotObservable(
                OnNext(ONE_MINUTE_TICKS * 2, "A"),
                OnNext(ONE_MINUTE_TICKS * 3, "B"),
                OnNext(ONE_MINUTE_TICKS * 4, "C"),
                OnNext(ONE_MINUTE_TICKS * 5, "D"),
                OnNext(ONE_MINUTE_TICKS * 10, "E"),
                OnCompleted<string>(ONE_MINUTE_TICKS * 11));

            // System under test
            var xs = from w in hotSource.Window(
                                TimeSpan.FromMinutes(3), 
                                _scheduler)
                     from sum in w.Sum(f => f[0])
                     select sum;

            // act
            //var observer = _scheduler.Start(() => xs);

            var observer = _scheduler.Start(() => xs,
                // DON'T USE THE DEFAULT
                created:0, 
                subscribed:0,
                disposed:ONE_MINUTE_TICKS * 12 /* dispose after completion */);

            observer.Messages.AssertEqual(
                OnNext(ONE_MINUTE_TICKS * 3 + 1, 'A' + 'B'),
                OnNext(ONE_MINUTE_TICKS * 6 + 1, 'C' + 'D'),
                OnNext(ONE_MINUTE_TICKS * 9 + 1, 0), // empty window
                OnNext<int>(ONE_MINUTE_TICKS * 11, 'E'),
                OnCompleted<int>(ONE_MINUTE_TICKS * 11)
            );
        }

        #endregion // PresetAndPostset_Test

        #region PresetAndAdvance_Test

        [TestMethod]
        public void PresetAndAdvance_Test()
        {
            // arrange
            var hotSource = _scheduler.CreateHotObservable(
                OnNext(ONE_MINUTE_TICKS * 2, "A"),
                OnNext(ONE_MINUTE_TICKS * 3, "B"),
                OnNext(ONE_MINUTE_TICKS * 4, "C"),
                OnNext(ONE_MINUTE_TICKS * 5, "D"),
                OnNext(ONE_MINUTE_TICKS * 10, "E"),
                OnCompleted<string>(ONE_MINUTE_TICKS * 11));

            var observer = _scheduler.CreateObserver<int>();

            // System under test
            var xs = from w in hotSource.Window(TimeSpan.FromMinutes(3), _scheduler)
                     from sum in w.Sum(f => f[0])
                     select sum;

            // act
            xs.Subscribe(observer);

            _scheduler.AdvanceTo(ONE_MINUTE_TICKS * 6 + 1);

            observer.Messages.AssertEqual(
                OnNext<int>(ONE_MINUTE_TICKS * 3, 'A' + 'B'),
                OnNext<int>(ONE_MINUTE_TICKS * 6, 'C' + 'D')
            );

            _scheduler.AdvanceBy(ONE_MINUTE_TICKS * 5);

            observer.Messages.AssertEqual(
                OnNext<int>(ONE_MINUTE_TICKS * 3, 'A' + 'B'),
                OnNext<int>(ONE_MINUTE_TICKS * 6, 'C' + 'D'),
                OnNext<int>(ONE_MINUTE_TICKS * 9, 0), // empty window
                OnNext<int>(ONE_MINUTE_TICKS * 11, 'E'),
                OnCompleted<int>(ONE_MINUTE_TICKS * 11)
            );
        }

        #endregion // PresetAndAdvance_Test

        #region TenStars_ShouldProduce10StarsAndComplete_Test

        [TestMethod]
        public void TenStars_ShouldProduce10StarsAndComplete_Test()
        {
            // arrange
            var observer = _scheduler.CreateObserver<string>();
            
            // act
            var stars = _instance.TenStars();
            stars.Subscribe(observer);

            // verify
            _scheduler.AdvanceBy(ONE_MINUTE_TICKS * RxOperation.LIMIT);

            var values = from rec in observer.Messages
                         where rec.Value.Kind == NotificationKind.OnNext
                         select rec;

            Assert.AreEqual(RxOperation.LIMIT, values.Count(), "Item count");
            for (int i = 0; i < RxOperation.LIMIT; i++)
			{
                Assert.AreEqual((i + 1) * ONE_MINUTE_TICKS, observer.Messages[i].Time, "check the timing");
			}

            Assert.AreEqual(NotificationKind.OnCompleted, observer.Messages.Last().Value.Kind);
        }

        #endregion // TenStars_ShouldProduce10StarsAndComplete_Test

        #region TenStars_ShouldProduce10StarsAndComplete_BySteps_Test

        [TestMethod]
        public void TenStars_ShouldProduce10StarsAndComplete_BySteps_Test()
        {
            // arrange
            var observer = _scheduler.CreateObserver<string>();
            
            // act
            var stars = _instance.TenStars();
            stars.Subscribe(observer);

            // verify
            var values = from rec in observer.Messages
                         where rec.Value.Kind == NotificationKind.OnNext
                         select rec;

            for (int i = 0; i < RxOperation.LIMIT; i++)
            {
                _scheduler.AdvanceBy(ONE_MINUTE_TICKS );
                Assert.AreEqual((i + 1) * ONE_MINUTE_TICKS, 
                    observer.Messages.Last().Time, "check the timing");
            }

            Assert.AreEqual(RxOperation.LIMIT, values.Count(), "Item count");
            Assert.AreEqual(NotificationKind.OnCompleted, observer.Messages.Last().Value.Kind);
        }

        #endregion // TenStars_ShouldProduce10StarsAndComplete_BySteps_Test

        #region TenStars_ShouldProduce10StarsAndComplete_BySteps_CompareWithPreset_Test

        [TestMethod]
        public void TenStars_ShouldProduce10StarsAndComplete_BySteps_CompareWithPreset_Test()
        {
            // arrange
            
            // act
            var observer = _scheduler.Start(
                () => _instance.TenStars(),
                0, 0, ONE_MINUTE_TICKS * 20 /* long after completion*/);

            // verify
            observer.Messages.AssertEqual(
                OnNext(ONE_MINUTE_TICKS * 1 + 1, "*"),
                OnNext(ONE_MINUTE_TICKS * 2 + 1, "**"),
                OnNext(ONE_MINUTE_TICKS * 3 + 1, "***"),
                OnNext(ONE_MINUTE_TICKS * 4 + 1, "****"),
                OnNext(ONE_MINUTE_TICKS * 5 + 1, "*****"),
                OnNext(ONE_MINUTE_TICKS * 6 + 1, "******"),
                OnNext(ONE_MINUTE_TICKS * 7 + 1, "*******"),
                OnNext(ONE_MINUTE_TICKS * 8 + 1, "********"),
                OnNext(ONE_MINUTE_TICKS * 9 + 1, "*********"),
                OnNext(ONE_MINUTE_TICKS * 10 + 1, "**********"),
                OnCompleted<string>(ONE_MINUTE_TICKS * 10 + 1)
            );

        }

        #endregion // TenStars_ShouldProduce10StarsAndComplete_BySteps_CompareWithPreset_Test

        #region Schedule_Start_Test

        [TestMethod]
        public void Schedule_Start_Test()
        {
            // arrange
            int i = 0;
            _scheduler.Schedule(TimeSpan.FromTicks(10), () => i++);
            _scheduler.Schedule(TimeSpan.FromTicks(20), () => i++);
            
            // act
            _scheduler.Start();

            // verify
            Assert.AreEqual(2, i);
            Assert.AreEqual(20, _scheduler.Clock);
        }

        #endregion // Schedule_Start_Test

        #region Schedule_Nested_Start_Test

        [TestMethod]
        public void Schedule_Nested_Start_Test()
        {
            // arrange
            int i = 0;
            _scheduler.Schedule(TimeSpan.FromTicks(10), () =>
                {
                    i++;
                    _scheduler.Schedule(TimeSpan.FromTicks(20), () => i++);
                });
            
            // act
            _scheduler.Start();

            // verify
            Assert.AreEqual(2, i);
            Assert.AreEqual(30, _scheduler.Clock);
        }

        #endregion // Schedule_Nested_Start_Test

        #region Schedule_StartAndStop_Test

        [TestMethod]
        public void Schedule_StartAndStop_Test()
        {
            // arrange
            int i = 0;
            _scheduler.Schedule(TimeSpan.FromTicks(10), () => i++);
            _scheduler.Schedule(TimeSpan.FromTicks(20), () => _scheduler.Stop());
            _scheduler.Schedule(TimeSpan.FromTicks(30), () => i++);

            // act & verify
            _scheduler.Start();
            Assert.AreEqual(1, i);
            Assert.AreEqual(20, _scheduler.Clock);

            _scheduler.Start();
            Assert.AreEqual(2, i);
            Assert.AreEqual(30, _scheduler.Clock);
        }

        #endregion // Schedule_StartAndStop_Test
    }
}
