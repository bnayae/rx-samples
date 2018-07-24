using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Immutable;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Reactive;
using static Bnaya.Samples.Constatnts;
using System.Windows;
using System.Diagnostics;

namespace Bnaya.Samples
{
    public class ViewModel
    {
        private readonly static TimeSpan GROUPING_WIN = TimeSpan.FromMilliseconds(20);
        private readonly static TimeSpan MISSILE_SPEED = TimeSpan.FromMilliseconds(10);
        private const int MISSILE_VELOCITY = 10;
        private const int DEF_STEP = 40;
        private ConcurrentDictionary<int, Invader> _invadersUiState = new ConcurrentDictionary<int, Invader>();
        private ConcurrentDictionary<int, Item> _defenderMissilesState = new ConcurrentDictionary<int, Item>();
        //private readonly Subject<Item> _invaders = new Subject<Item>();
        //private readonly BehaviorSubject<double> _speedSubject = new BehaviorSubject<double>(0.5);
        //private readonly BehaviorSubject<Item> _defenderUiState;
        //private IObservable<IObservable<Item>> _defenderMissiles;
        //private double DELAY_MILLISEC = 0.1;
        private readonly TimeSpan _frameRate;

        #region Ctor

        public ViewModel(
            IObservable<DefenderAction> userActions,
            TimeSpan frameRate)
        {
            _frameRate = frameRate;

            #region Defender Query

            Defender = new Item(Kind.Defender, BorderWidth / 2, BorderHeight / 2 - Item.DEFAULT_SIZE, Item.DEFAULT_SIZE * 2);
            var defenderStream = userActions
                    .Where(m => m == DefenderAction.Right || m == DefenderAction.Left)
                    .Do(m =>
                    {
                        Item defender = Defender;
                        if (m == DefenderAction.Right)
                            defender.X += 1;
                        else if (m == DefenderAction.Left)
                            defender.X -= 1;

                        if (defender.X < 0)
                            defender.X = 0;
                        if (defender.X > BorderWidth - Defender.Size)
                            defender.X = BorderWidth - Defender.Size;
                    })
                    .Select(_ => Defender)
                    .Where(m => !m.IsHit)
                    .Publish();
            DefenderStream = defenderStream;
            defenderStream.Connect();

            #endregion // Defender Query

            #region Defender Missile Query

            var defenderMissileStream = userActions
                     .Where(m => m == DefenderAction.Fire)
                     .Sample(TimeSpan.FromSeconds(0.3))
                     .SelectMany(m => CreateDefenderMissile())
                     .Publish();
            DefenderMissileStream = defenderMissileStream;
            defenderMissileStream.Connect();

            #endregion // Defender Missile Query

            #region Invaders

            // accelerate the pace every 4 second 
            double framesInSec = 4000 / _frameRate.TotalMilliseconds;
            //IConnectableObservable<int> stepDistancePerFrame =
            //    Observable.Interval(_frameRate)
            //              // single step distance (correlate to the frame-rate)
            //              .Select(cur => Math.Min(10, (int)(cur / framesInSec) + 1))
            //              .Publish();

            IConnectableObservable<int> stepDistancePerFrame =
                (from speed in Observable.Timer(
                                                TimeSpan.Zero, TimeSpan.FromSeconds(4))
                                                .Take(5)
                     select Observable.Interval(_frameRate)
                     .Select(m => (int)speed + 1))
                     .Switch()
                     .Publish();

            var invaderTriggerStream = // cold stream that trigger when the total pace > x
                stepDistancePerFrame.Scan((p, c) => p + c) 
                                    .FirstAsync(m => m > Item.DEFAULT_SIZE * 3) // when total distance > x trigger to add invader
                                    .Repeat(); // resubscribe

            var invaderStream = from _ in stepDistancePerFrame
                                            .Sample(invaderTriggerStream)
                                where BorderWidth > 0 && BorderHeight > 0
                                select new Invader(stepDistancePerFrame,
                                                            () => new Size(BorderWidth, BorderHeight));
            var invaderConnectableStream = invaderStream
                                .Do(m =>
                                {
                                    int k = m.Id;
                                    if (!_invadersUiState.TryAdd(k, m))
                                        Trace.WriteLine("Invader addition failed");
                                    //m.TakeWhile(m1 => !m1.IsHit)
                                    m.IgnoreElements()
                                      .Subscribe(
                                        _ => { },
                                        () =>
                                        {
                                            if (!_invadersUiState.TryRemove(k, out m))
                                                Trace.WriteLine("Fail to remove invader");
                                        });
                                }).SelectMany(m => m)
                                .Publish();

            stepDistancePerFrame.Connect();
            InvaderStream = invaderConnectableStream;
            invaderConnectableStream.Connect();

            #endregion // Invaders

            HitDetections();
        }

        #endregion // Ctor

        private IObservable<Item> DefenderStream { get; }
        private IObservable<Item> DefenderMissileStream { get; }
        private IObservable<Item> InvaderStream { get; }

        private void HitDetections()
        {
            Observable.CombineLatest(
                            InvaderStream,
                            DefenderMissileStream)
                            //.ObserveOn(TaskPoolScheduler.Default)
                      .Where(pair => !pair[0].IsHit && !pair[1].IsHit)
                      .Where(pair => pair[0].IsIntersect(pair[1]))
                      .Subscribe(pair => { pair[0].Hit(); pair[1].Hit(); });
        }

        #region InvadersState

        /// <summary>
        /// Gets or sets the state of the invaders.
        /// </summary>
        /// <value>
        /// The state of the invaders.
        /// </value>
        public IEnumerable<Item> InvadersState => _invadersUiState.Values.ToArray();

        #endregion // InvadersState

        #region DefenderMissilesState

        public IEnumerable<Item> DefenderMissilesState => _defenderMissilesState.Select(m => m.Value).ToArray();

        #endregion // DefenderMissilesState

        #region Defender

        public Item Defender { get; }

        #endregion // Defender

        #region CreateDefenderMissile

        private IObservable<Item> CreateDefenderMissile()
        {
            var missile = new Item(
                    Kind.DefenderMissile,
                    Defender.X + Defender.Size / 2, Defender.Y - Defender.Size, Defender.Size / 2);
            _defenderMissilesState.TryAdd(missile.Id, missile);
            var missiles = Observable.Generate(
                        missile,
                        m => !m.IsHit && m.Y > 0,
                        m => { /*if(m.Y > 30)*/ m.Y -= MISSILE_VELOCITY; return m; },
                        m => m,
                        m => _frameRate)
                        .Publish(); // avoid multiple manipulation
            missiles.Connect();
            missiles.DefaultIfEmpty()
                    .LastAsync()
                    .Subscribe(m =>
                        {
                            if (!_defenderMissilesState.TryRemove(m.Id, out m))
                                Trace.WriteLine("Fail to remove");
                        });
            return missiles;
        }

        #endregion // CreateDefenderMissile

        #region Advance

        public void Advance(Item invader, double step)
        {
            if (BorderWidth == 0)
                return;

            invader.X += step;
            if (invader.X > BorderWidth - 40)
            {
                invader.X = 0;
                invader.Y += invader.Size * 2;
            }

            if (invader.Y >= Defender.Y)
                Defender.Hit();
        }

        #endregion // Advance
    }
}