using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class ScheduleResponse : Response
    {
        public ScheduleResponse()
            : base()
        {
        }

        private Schedule schedule;

        public Schedule Schedule
        {
            get { return this.schedule; }
            set
            {
                this.schedule = value;
            }
        }
    }
}
