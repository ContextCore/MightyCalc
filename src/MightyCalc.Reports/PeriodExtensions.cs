using System;

namespace MightyCalc.Reports
{
    
    public static class PeriodExtensions
    {
        public static readonly TimeSpan OneMinute = TimeSpan.FromMinutes(1);

        public static DateTimeOffset ToPeriodEnd(this DateTimeOffset time, TimeSpan period)
        {
            return time.ToPeriodBegin(period) + period;
        }
        
        public static DateTimeOffset ToPeriodBegin(this DateTimeOffset time, TimeSpan period)
        {
            var periodMilliSeconds = period.Ticks;
            var timeTicks = time.Ticks;
            var periodStartTicks = timeTicks - timeTicks % periodMilliSeconds;
            return new DateTimeOffset(periodStartTicks, time.Offset);
        }
        
        public static DateTimeOffset ToMinutePeriodEnd(this DateTimeOffset time)
        {
            return time.ToPeriodEnd(OneMinute);
        }
        
        public static DateTimeOffset ToMinutePeriodBegin(this DateTimeOffset time)
        {
            return time.ToPeriodBegin(OneMinute);
        }
    }
}