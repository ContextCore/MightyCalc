using System;

namespace MightyCalc.Reports.Streams
{
    public static class Period
    {
        public static DateTimeOffset Begin(TimeSpan duration, DateTimeOffset eventTime)
        {
            var periodMilliSeconds = duration.Ticks;
            var timeTicks = eventTime.Ticks;
            var periodStartTicks = timeTicks - timeTicks % periodMilliSeconds;
            return new DateTimeOffset(periodStartTicks, eventTime.Offset);
        }
        
        public static DateTimeOffset End(TimeSpan duration, DateTimeOffset eventTime)
        {
            return Begin(duration, eventTime) + duration;
        }
    }
}