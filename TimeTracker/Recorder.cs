using System;

namespace TimeTracker
{
    public struct Recorder
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsTimerRunning { get; set; }

        public TimeSpan getTotalDuration()
        {
            var start = StartTime;
            var stop = EndTime;
            return stop.Subtract(start).StripMilliseconds();
        }
    }
}
