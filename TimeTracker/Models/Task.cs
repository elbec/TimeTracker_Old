using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TimeTracker
{
    public class Task
    {
        public int id { get; set; }
        public String title { get; set; }
        public String subtitle { get; set; }
        public String createDate { get; set; }
        public ICollection<Recorder> Recorders { get; set; }
    }
}
