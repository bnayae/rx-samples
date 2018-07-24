using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bnaya.Samples
{
    public sealed class FileObservable: IObservable<char>, IDisposable
    {
        private readonly FileSystemWatcher _fsw;
        private readonly Subject<char> _subject = new Subject<char>();
        private int _disposed = -1;

        #region Ctor

        public FileObservable(string folder)
            :this(new FileSystemWatcher(Path.GetFullPath(folder)))
        {
        }

        public FileObservable(FileSystemWatcher fsw)
        {
            fsw.Changed += OnChanged;
            fsw.Deleted += OnDeleted;
            fsw.EnableRaisingEvents = true;
            _fsw = fsw;
        }

        #endregion // Ctor

        #region Subscribe

        public IDisposable Subscribe(IObserver<char> observer)
        {
            return _subject.Subscribe(observer);
        }

        #endregion // Subscribe

        #region OnChanged

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Task _ = OnChangedAsync();
            async Task OnChangedAsync()
            {
                string data = null;
                for (int i = 0; i < 4; i++)
                {
                    try
                    {
                        data = File.ReadAllText(e.FullPath);
                        break;
                    }
                    catch { Trace.WriteLine("File is locked... retry"); }
                    await Task.Delay(10);
                }
                if (!string.IsNullOrEmpty(data))
                    _subject.OnNext(data.Last());
            }
        }

        #endregion // OnChanged

        #region OnDeleted

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Dispose();
        }

        #endregion // OnDeleted

        #region Dispose Pattern

        public void Dispose()
        {
            #region Validation

            if (Interlocked.Increment(ref _disposed) != 0)
                return; // ensure dispose once

            #endregion // Validation

            _subject.OnCompleted();
            _fsw.Changed -= OnChanged;
            _fsw.Deleted -= OnDeleted;
            _fsw.Dispose();
            _subject.Dispose();
            GC.SuppressFinalize(this);
        }

        ~FileObservable() => Dispose();

        #endregion // Dispose Pattern
    }
}
