#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;

#endregion // Using

// https://github.com/bnayae/Rx.NET/blob/01061232f0b426763682da65d49a6c4f036927ca/Rx.NET/Source/src/System.Reactive/Concurrency/Scheduler.Services.Emulation.cs

namespace Bnaya.Samples
{
    public class TimeAccelerateScheduler : IScheduler, ISchedulerPeriodic, IServiceProvider
    {
        private DateTimeOffset _baselineTime; 
        private double _accelerateFactor;
        /// <summary>
        /// will handle the Thread affinity
        /// </summary>
        private IScheduler _scheduler;

        #region Constructors

        /// <summary>
        /// initialize the scheduler by the accelerate factor
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="scheduler">
        /// will handle the Thread affinity
        /// the default is task pool.
        /// </param>
        public TimeAccelerateScheduler(double factor, IScheduler scheduler = null)
        {
            _accelerateFactor = factor;
            _scheduler = scheduler;
            if (_scheduler == null)
                _scheduler = TaskPoolScheduler.Default;
        }

        #endregion // Constructors

        #region IServiceProvider.GetService

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>
        /// A service object of type <paramref name="serviceType"/>.-or- null if there is no service object of type <paramref name="serviceType"/>.
        /// </returns>
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(ISchedulerPeriodic))
                return this;
            return null;
        }

        #endregion // IServiceProvider.GetService

        #region Now

        /// <summary>
        /// Return virtual time (accelerate time)
        /// </summary>
        public DateTimeOffset Now
        {
            get 
            {
                return GetAccelerateTime(DateTimeOffset.Now);
            }
        }

        #endregion // Now

        #region Schedule

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            Console.Write("#");
            return AccelerateSchedule(state, dueTime, action);
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            Console.Write("@");
            return AccelerateSchedule(state, dueTime, action);
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            Console.Write("*");
            return AccelerateSchedule<TState>(state, DateTimeOffset.Now, action);
        }

        #endregion // Schedule

        #region ISchedulerPeriodic.SchedulePeriodic

        public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action)
        {
            if (period < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(period));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            TimeSpan targetTime = GetAccelerateTime(period);
            var state1 = state;
            var gate = new AsyncLock();
            var cancel = new Timer(s => 
            {
                gate.Wait(() =>
                {
                    state1 = action(state1);
                });
            }, null, targetTime, targetTime);
            var gcHandler = GCHandle.Alloc(cancel, GCHandleType.Normal);

            return Disposable.Create(() =>
            {
                gcHandler.Free();
                cancel.Dispose();
                gate.Dispose();
                action = null;
            });
        }

        //public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action)
        //{
        //    Console.WriteLine("Periodic scheduling");
        //    TimeSpan targetTime = GetAccelerateTime(period);
        //    return _scheduler.SchedulePeriodic(state, targetTime, action);        
        //}

        #endregion // ISchedulerPeriodic.SchedulePeriodic

        #region AccelerateSchedule

        private IDisposable AccelerateSchedule<TState>(
            TState state,
            TimeSpan dueTime,
            Func<IScheduler, TState, IDisposable> action)
        {
            TimeSpan targetTime = GetAccelerateTime(dueTime);
            return _scheduler.Schedule<TState>(state, targetTime, (scd, state_) => action(this, state_));
        }

        private IDisposable AccelerateSchedule<TState>(
            TState state, 
            DateTimeOffset dueTime, 
            Func<IScheduler, TState, IDisposable> action)
        {
            DateTimeOffset targetTime = GetAccelerateTime(dueTime);

            return _scheduler.Schedule<TState>(state, targetTime, (scd, state_) => action(this, state_));
        }

        #endregion // AccelerateSchedule

        #region GetAccelerateTime

        /// <summary>
        /// Gets an absolute accelerate time.
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        /// <returns></returns>
        private DateTimeOffset GetAccelerateTime(DateTimeOffset dueTime)
        {
            if (_baselineTime == DateTimeOffset.MinValue)
                _baselineTime = _scheduler.Now; // set the scheduler base-line time

            double fromOffset = (dueTime - _baselineTime).TotalMilliseconds;
            double accelerateTime = fromOffset * _accelerateFactor; // accelerate timing
            DateTimeOffset targetTime = _baselineTime.AddMilliseconds(accelerateTime);

            return targetTime;
        }

        /// <summary>
        /// Gets a relative accelerate time.
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        /// <returns></returns>
        private TimeSpan GetAccelerateTime(TimeSpan dueTime)
        {
            double fromOffset = dueTime.TotalMilliseconds;
            double accelerateTime = fromOffset * _accelerateFactor; // accelerate timing
            TimeSpan targetTime = TimeSpan.FromMilliseconds(accelerateTime);

            return targetTime;
        }

        #endregion // GetAccelerateTime
    }
}
