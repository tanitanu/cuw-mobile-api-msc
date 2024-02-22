using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping.AwardCalendar
{
   
    [Serializable()]
    public class MOBSHOPAwardCabinTypeCalendar
    {
        private List<MOBSHOPAwardCalendarDay> calendarWeek;

        public List<MOBSHOPAwardCalendarDay> CalendarWeek
        {
            get { return calendarWeek; }
            set { calendarWeek = value; }
        }
        private string cabinType;
        public string CabinType
        {
            get { return cabinType; }
            set { cabinType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private bool isDefaultSelected;

        public bool IsDefaultSelected
        {
            get { return isDefaultSelected; }
            set { isDefaultSelected = value; }
        }


    }
}
