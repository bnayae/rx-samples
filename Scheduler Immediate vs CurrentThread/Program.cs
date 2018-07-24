#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion // Using

namespace Bnaya.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Test("Return (default)", Observable.Return(1));
            Test("Range (default)", Observable.Range(1, 1));

            #region Hide

            Console.Clear();

            TestWithTrace(Scheduler.Immediate);
            TestWithTrace(Scheduler.CurrentThread);


            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Completion of stream will unsubscribe its observers, ");
            Console.WriteLine("therefore no scheduled OnNext will occurs after completion");
            Console.ResetColor();
            Console.ReadKey();

            Console.Clear();

            ScheduleRec(Scheduler.Immediate);
            ScheduleRec(Scheduler.CurrentThread);

            #endregion // Hide
        }

        #region Test

        private static void Test(string title, IObservable<int> source)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\r\n############  {0}  ############\r\n", title);
            Console.ResetColor();

            source = source.Publish().RefCount(); // Hot stream

            source = from win in source.Window(source, opener => source)
                     from item in win.Count()
                     select item;

            source.Subscribe(v => Console.WriteLine("Count = {0}", v), () => Console.WriteLine("Complete"));

            Console.WriteLine();
            Console.WriteLine("Press any key");
            Console.ReadKey(true);
        }

        #endregion // Test

        #region TestWithTrace

        private static void TestWithTrace(IScheduler scd)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\r\n############  {0}  ############\r\n", scd.GetType().Name);
            Console.ResetColor();

            var source = Observable.Range(1, 1, scd);

            source = source.Publish().RefCount(); // hot stream

            source = source.Do(v => Write("Source: item = ", v, ConsoleColor.Yellow), () => Write("Source: ", "Complete"));

            var openStream = source.Do(v => Write("\t Open: item = ", v)); // doesn't matter, () => Write("\t\t Open", "Complete"));

            var closeStream = source.Do(v => Write("\t Close: item = ", v), () => Write("\t Close: ", "Complete"));

            source = from win in source.Window(openStream, opener => closeStream)
                        .Do(v => Write("\t\t Win: ", "Start", ConsoleColor.Magenta), () => Write("\t\t All Wins: ", "Complete", ConsoleColor.Magenta))
                     let w = win.Do(v => Write("\t\t\tWin: item = ", v, ConsoleColor.Green), () => Write("\t\t\tWin: ", "Complete", ConsoleColor.Green))
                     from item in w.Count()
                                    .Do(v => Write("\t\t\t\tCount = ", v, ConsoleColor.DarkYellow))
                     select item;

            source.Subscribe(v => Write("Output: count = ", v), () => Write("Output: ", "Complete"));

            Console.WriteLine();
            Console.ReadKey(true);
        }

        #endregion // TestWithTrace

        #region ScheduleRec

        private static void ScheduleRec(IScheduler scd)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\r\n############  {0}  ############\r\n", scd.GetType().Name);
            Console.ResetColor();

            scd.Schedule(
               () =>
               {
                   Console.WriteLine("1"); // right away

                   scd.Schedule(
                     () =>
                     {
                         // immediate (direct invocation) will execute it before 2 
                         // CurrentThread (queue) will execute it after 2 
                         //  (on completion of the scheduling it will pop the queue)
                         Console.WriteLine("3");

                         scd.Schedule(
                           () =>
                           {
                               // immediate (direct invocation) will execute it before 4 
                               // CurrentThread (queue) will execute it after 4 
                               //  (on completion of the scheduling it will pop the queue)
                               Console.WriteLine("5");
                           });

                         Console.WriteLine("4");
                     });

                   Console.WriteLine("2");
               });

            Console.WriteLine();
            Console.ReadKey(true);
        }

        #endregion // ScheduleRec

        #region Write

        private static void Write(string title, object data, ConsoleColor c = ConsoleColor.White)
        {
            Console.ForegroundColor = c;
            Console.Write("{0}{1} ", title, data);
            Console.ResetColor();
            Console.WriteLine("(Thread = {0})", Thread.CurrentThread.ManagedThreadId);
        }

        #endregion // Write
    }
}
