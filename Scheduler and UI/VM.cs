using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Input;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Reactive.Subjects;

namespace Bnaya.Samples
{
    public class VM: ICommand, INotifyPropertyChanged
    {
        //private readonly SynchronizationContextScheduler _scheduler = 
        //    new SynchronizationContextScheduler(SynchronizationContext.Current);

        private readonly Subject<string> _textSubject = new Subject<string>();
        private readonly BehaviorSubject<string> _textChangedSubject = 
            new BehaviorSubject<string>(string.Empty);

        public VM()
        {
            _textSubject.DistinctUntilChanged(m => m?.LastOrDefault())
                //.Do(m => OnPropertyChanged(nameof(ChangedText)))
                .Subscribe(_textChangedSubject);

            _textChangedSubject.Do(m => OnPropertyChanged(nameof(ChangedText)))
                                .Subscribe();
        }



        public ObservableCollection<long> Data { get; } = new ObservableCollection<long>();

        public event EventHandler CanExecuteChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string ChangedText => _textChangedSubject.Value;
        //public string ChangedText { get { return _textChangedSubject.Value; } }

        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                _textSubject.OnNext(value);
                OnPropertyChanged();
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Task t = ExecuteAsync();
        }


        public async Task ExecuteAsync()
        {
            await Task.Delay(1000);
            Data.Add(1000);
            Observable.Interval(TimeSpan.FromSeconds(1) , Scheduler.Default)
                      .Take(10)
                      .Do(m => Trace.WriteLine("Busy work"))
                      .ObserveOn(SynchronizationContext.Current)
                      .Subscribe(v => Data.Add(v), 
                                 ex => Trace.WriteLine(ex.ToString()));
        }
    }
}
