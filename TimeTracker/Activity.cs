using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker
{
    class Activity
    {
        public String Title;
        public String Subtitle;
        public DateTime StartTime;
        public DateTime EndTime;

        public TimeSpan getDuration()
        {
            return EndTime - StartTime;
        }

    }
}
