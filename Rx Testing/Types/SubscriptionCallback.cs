using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnaya.Samples
{
    public static class SubscriptionCallbackExtensions
    {
        public static IObservable<T> SubscriptionMonitor<T>(this IObservable<T> source, Action act)
        {
            return new SubscriptionCallback<T>(act, source);
        }

        private class SubscriptionCallback<T> : IObservable<T>
        {
            private readonly Action _act;
            private readonly IObservable<T> _source;

            public SubscriptionCallback(Action act, IObservable<T> source)
            {
                _act = act;
                _source = source;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                _act();
                return _source.Subscribe(observer);
            }
        }
    }
}
