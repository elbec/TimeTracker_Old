using System;

namespace TimeTracker
{
    public class Recorder
    {
        public int RecorderId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsTimerRunning { get; set; }

    }
}
