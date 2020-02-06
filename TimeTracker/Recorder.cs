using System;

namespace TimeTracker
{
    struct Recorder
    {
        public DateTime StartTime;
        public DateTime EndTime;
        public bool isTimerRunning;

        public TimeSpan getTotalDuration()
        {
            if (StartTime != null)
            {
                var start = StartTime;
                var stop = EndTime;

                return stop.Subtract(start).StripMilliseconds();
            }
            return TimeSpan.FromMinutes(0);
        }
    }
}
