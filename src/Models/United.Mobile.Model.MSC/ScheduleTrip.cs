using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
using System.Text;

namespace United.Definition
{

    [Serializable()]
    public class ScheduleTrip : FlightTrip
    {

        private List<ScheduleSegment> scheduleSegments;
        private bool[] operates; 

        public List<ScheduleSegment> ScheduleSegments
        {
            get
            {
                return this.scheduleSegments;
            }
            set
            {
                this.scheduleSegments = value;
            }
        }

       
        public bool[] Operates 
        {
            get
            {
                return this.operates;
            }
            set
            {
                this.operates = value;
            }
        }
    }
}
