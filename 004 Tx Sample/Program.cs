using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tx.Windows;
using System.Reactive.Linq;
using System.Reactive.Contrib.Monitoring;
using System.Reactive.Contrib.Monitoring.Contracts;

namespace Bnaya.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Start");

            IObservable<PerformanceSample> performanceCounterObservable =
                    PerfCounterObservable.FromRealTime(
                    TimeSpan.FromSeconds(1),
                    new[]
                    {
                        @"\Processor(_Total)\% Processor Time",
                        @"\Memory(_Total)\% Committed Bytes In Use",
                        @"\Memory(_Total)\Available MBytes"
                    });
            
            VisualRxInitResult info = await VisualRxSettings.Initialize(
                    VisualRxWcfDiscoveryProxy.Create());
            Console.WriteLine(info);

            performanceCounterObservable
                .Monitor("Step 1", 1)
                .Where(v => v.CounterName == "% Processor Time")
                .Monitor("Step 2", 1)
                .Select(m => (int)(m.Value / 10))
                .Select(m => new string('-', m))
                .Subscribe(
                v => Console.WriteLine(v));
    
            Console.ReadKey();
        }
    }
}
