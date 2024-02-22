using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBScheduleResponse : MOBResponse
    {
        public MOBScheduleResponse()
            : base()
        {
        }

        private MOBSchedule schedule;

        public MOBSchedule Schedule
        {
            get { return this.schedule; }
            set
            {
                this.schedule = value;
            }
        }
    }
}
