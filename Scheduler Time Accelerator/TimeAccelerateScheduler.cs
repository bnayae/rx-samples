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

#endregion // Using

namespace Bnaya.Samples
{
    public class TimeAccelerateScheduler : IScheduler
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
            _scheduler = scheduler ?? Scheduler.Default;
        }

        #endregion // Constructors

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
            return AccelerateSchedule(state, dueTime, action);
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return AccelerateSchedule(state, dueTime, action);
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            return AccelerateSchedule<TState>(state, DateTimeOffset.Now, action);
        }

        #endregion // Schedule

        #region AccelerateSchedule

        private IDisposable AccelerateSchedule<TState>(
            TState state,
            TimeSpan dueTime,
            Func<IScheduler, TState, IDisposable> action)
        {
            TimeSpan targetTime = GetAccelerateTime(dueTime);
            return _scheduler.Schedule<TState>(state, targetTime, 
                                    (scd, state_) => action(this, state_));
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
