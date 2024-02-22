using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBFlifoCitySearchResponse : MOBResponse
    {
        public MOBFlifoCitySearchResponse()
            : base()
        {
        }

        private MOBFlifoCitySearchSchedule schedule;

        public MOBFlifoCitySearchSchedule Schedule
        {
            get { return this.schedule; }
            set
            {
                this.schedule = value;
            }
        }
        //public MOBFlifoScheduleTrip[] Schedule;
    }
}
