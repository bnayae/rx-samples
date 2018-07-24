#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reactive.Concurrency;

#endregion // Using

namespace Bnaya.Samples
{
    /// <summary>
    /// this scheduler present the time with 'I' char for each minute
    /// Absolute Time present by string i.e. "III" = 3 minutes
    /// Relative time present by long 3 = 3 minutes
    /// </summary>
    public class PrisonScheduler : VirtualTimeScheduler<string, long>
    {
        private DateTimeOffset _startTime = DateTimeOffset.Now;

        protected override string Add(string absolute, long relative)
        {
            StringBuilder sb = new StringBuilder(absolute); // build the char projection
            for (int i = 0; i < relative; i++)
            {
                sb.Append("I");
            }
            return sb.ToString();
        }

        protected override long ToRelative(TimeSpan timeSpan)
        {
            return Convert.ToInt64(timeSpan.TotalMinutes);
        }

        protected override DateTimeOffset ToDateTimeOffset(string absolute)
        {
            if (string.IsNullOrEmpty(absolute))
                return _startTime;

            return _startTime.AddMinutes(absolute.Length);
        }
    }
}
