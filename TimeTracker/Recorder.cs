using System;
using System.Windows.Threading;

namespace TimeTracker
{
    public struct Recorder
    {
        public DateTime StartTime;
        public DateTime EndTime;
        public bool isTimerRunning;
        public DispatcherTimer timer;
        public int counter;

        public TimeSpan getTotalDuration()
        {
            if (StartTime != null)
            {
                var start = StartTime;
                var stop = EndTime;

                return stop.Subtract(start).StripMilliseconds();
            } else
                return TimeSpan.FromMinutes(0);
        }

        internal void timer_Tick(object sender, EventArgs e)
        {
            counter += 1;
        }
    }
}
