#region Using

using System;
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
    // BE aware that the this is unfair implementation that can easily cause starvation

    public class SimpleScheduler : IScheduler, ISchedulerLongRunning, ISchedulerPeriodic, IServiceProvider
    {
        public bool IsTargetLongRunning { get; private set; }
        public bool IsTargetPeriodic { get; private set; }

        #region IServiceProvider

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(ISchedulerPeriodic))
                return this;
            if (serviceType == typeof(ISchedulerLongRunning))
                return this;

			return null;         
        }
 
        #endregion // IServiceProvider

        #region Now

        /// <summary>
        /// Gets the scheduler's notion of current time.
        /// </summary>
        public DateTimeOffset Now
        {
            get { return DateTime.Now; }
        }

        #endregion // Now

        #region Schedule

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return action(this, state);
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return action(this, state);
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            return action(this, state);
        }

        #endregion // Schedule

        #region ISchedulerLongRunning members

        public IDisposable ScheduleLongRunning<TState>(TState state, Action<TState, ICancelable> action)
        {
            IsTargetLongRunning = true;
            var disp = new CancellationDisposable();
            Task.Run(() => action(state, disp), disp.Token);
            
            return disp;
        }

        #endregion // ISchedulerLongRunning members

        #region ISchedulerPeriodic members

        public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action)
        {
            IsTargetPeriodic = true;
            var disp = new BooleanDisposable();

            Task.Run(async () =>
                {
                    while (!disp.IsDisposed)
                    {
                        await Task.Delay(period);
                        state = action(state);
                    }
                });

            return disp;
        }

        #endregion // ISchedulerPeriodic members
   }
}
