using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnaya.Samples
{
    public class GenObservable<T> : IObserver<T>
    {
        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;
        private readonly Action _onComplete;

        public GenObservable(
            Action<T> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            _onNext = onNext;
            _onError = onError;
            _onComplete = onComplete;
        }
        public void OnCompleted() => _onComplete();

        public void OnError(Exception error) => _onError(error);

        public void OnNext(T value) => _onNext(value);
    }
}
