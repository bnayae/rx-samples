using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Reactive.Disposables;

namespace Bnaya.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            Console.WindowLeft = 0;
            Console.WindowTop = 0;

            CheckReturn();
            CheckRange();
            CheckInterval();
            CheckGenerate();
            CheckBuffer();
            CheckWindow();
        }

        #region CheckReturn

        private static void CheckReturn()
        {
            Console.WriteLine("**********************************************");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t Return");
            Console.ResetColor();
            Console.WriteLine("**********************************************\r\n");

            string non = Run(Observable.Return(1L), "No Scheduler");
            string def = Run(Observable.Return(1L,
                                               DefaultScheduler.Instance), "DefaultScheduler.Instance");
            string def1 = Run(Observable.Return(1L,
                                                Scheduler.Default), "Scheduler.Default");
            string real = Run(Observable.Return(1L, ImmediateScheduler.Instance), "ImmediateScheduler.Instance");

            WriteEquality(non, def);
        }

        #endregion // CheckReturn

        #region CheckBuffer

        private static void CheckBuffer()
        {
            Console.WriteLine("**********************************************");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t Buffer");
            Console.ResetColor();
            Console.WriteLine("**********************************************\r\n");

            var xs = Observable.Interval(TimeSpan.FromMilliseconds(1));

            string non = Run(xs.Buffer(TimeSpan.FromMilliseconds(1)), 2, "No Scheduler");
            string def = Run(xs.Buffer(TimeSpan.FromMilliseconds(1), Scheduler.Default), 3, "Scheduler.Default");

            WriteEquality(non, def);
        }

        #endregion // CheckBuffer

        #region CheckWindow

        private static void CheckWindow()
        {
            Console.WriteLine("**********************************************");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t Window");
            Console.ResetColor();
            Console.WriteLine("**********************************************\r\n");

            var xs = Observable.Interval(TimeSpan.FromMilliseconds(1));

            string non = Run(xs.Window(TimeSpan.FromMilliseconds(1)), 2);
            string def = Run(xs.Window(TimeSpan.FromMilliseconds(1), DefaultScheduler.Instance), 3);

            WriteEquality(non, def);
        }

        #endregion // CheckWindow

        #region CheckRange

        private static void CheckRange()
        {
            Console.WriteLine("**********************************************");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t Range");
            Console.ResetColor();
            Console.WriteLine("**********************************************\r\n");

            string non = Run(Observable.Range(1, 1).Select(i => (long)i));
            string def = Run(Observable.Range(1, 1, DefaultScheduler.Instance).Select(i => (long)i));

            WriteEquality(non, def);
        }

        #endregion // CheckRange

        #region CheckInterval

        private static void CheckInterval()
        {
            Console.WriteLine("**********************************************");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t Interval");
            Console.ResetColor();
            Console.WriteLine("**********************************************\r\n");

            string non = Run(Observable.Interval(TimeSpan.FromMilliseconds(1)));
            string def = Run(Observable.Interval(TimeSpan.FromMilliseconds(1), DefaultScheduler.Instance));

            WriteEquality(non, def);
        }

        #endregion // CheckInterval

        #region CheckGenerate

        private static void CheckGenerate()
        {
            Console.WriteLine("**********************************************");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t Generate");
            Console.ResetColor();
            Console.WriteLine("**********************************************\r\n");

            string non = Run(Gen());
            string def = Run(Gen(DefaultScheduler.Instance));

            WriteEquality(non, def);
        }

        private static IObservable<long> Gen(IScheduler scd = null)
        {
            if (scd == null)
                return Observable.Generate(1L, i => true, i => i, i => i);
            return Observable.Generate(1L, i => true, i => i, i => i, scd);
        }

        #endregion // CheckGenerate

        #region WriteEquality

        private static void WriteEquality(string non, string def)
        {
            if (non == def)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Equals");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Non-Equals");
            }
            Console.ResetColor();
            Console.ReadKey(true);
            Console.Clear();
        }

        #endregion // WriteEquality

        #region Run

        private static string Run<T>(
            IObservable<T> xs,
            string title)
        {
            return Run(xs, 1, title);
        }

        private static string Run<T>(
            IObservable<T> xs,
            int iterations = 1,
            string title = null)
        {
            if (title != null)
                Console.WriteLine($"## {title} ##");
            var sync = new ManualResetEventSlim();
            var sb = new StringBuilder();

            xs.Take(iterations).Subscribe(v =>
            {
                lock (sync)
                {
                    StackTrace st = new StackTrace(1, true);
                    for (int i = 0; i < st.FrameCount; i++)
                    {
                        Console.ForegroundColor = i % 2 == 0 ? ConsoleColor.White : ConsoleColor.Yellow;
                        // Note that high up the call stack, there is only 
                        // one stack frame.
                        StackFrame sf = st.GetFrame(i);
                        var mtd = sf.GetMethod();
                        var cls = mtd.ReflectedType.Name;
                        cls = cls.StartsWith("Anonymous") ? "() =>" : cls;
                        string data = string.Format("{0}: {1}, ", cls, mtd.Name);
                        Console.Write(data);
                        sb.AppendLine(data);
                        Trace.WriteLine(data);

                        if (i % 4 == 0)
                            Console.Write("\r\n\t");
                    }
                    Console.WriteLine();
                    Console.ResetColor();
                }
            }, () => sync.Set());

            sync.Wait();

            Console.WriteLine("---------------------------------------------------------------------");

            return sb.ToString();
        }

        #endregion // Run
    }


    #region Where to look

    /*
     
     
     namespace Bnaya.Samples
{
	internal static class SchedulerDefaults
	{
		internal static IScheduler ConstantTimeOperations
		{
			get
			{
				return ImmediateScheduler.Instance;
			}
		}
		internal static IScheduler TailRecursion
		{
			get
			{
				return ImmediateScheduler.Instance;
			}
		}
		internal static IScheduler Iteration
		{
			get
			{
				return CurrentThreadScheduler.Instance;
			}
		}
		internal static IScheduler TimeBasedOperations
		{
			get
			{
				return DefaultScheduler.Instance;
			}
		}
		internal static IScheduler AsyncConversions
		{
			get
			{
				return DefaultScheduler.Instance;
			}
		}
	}
}
     
   
     * 
     * 
     * 
// System.Reactive.Linq.QueryLanguage
public virtual IObservable<TResult> Return<TResult>(TResult value)
{
	return new Return<TResult>(value, SchedulerDefaults.ConstantTimeOperations);
}
     * 
     * 
 internal class QueryLanguage : IQueryLanguage

     
     */

    #endregion // Where to look
}
