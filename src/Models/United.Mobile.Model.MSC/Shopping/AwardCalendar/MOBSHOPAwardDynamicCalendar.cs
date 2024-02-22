using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Services.FlightShopping.Common;

namespace United.Definition.Shopping.AwardCalendar
{
    [Serializable()]
    public class MOBSHOPAwardDynamicCalendar
    { 

        private List<MOBSHOPAwardCabinTypeCalendar> awardcalendars;
        public List<MOBSHOPAwardCabinTypeCalendar> AwardCalendars
        {
            get { return awardcalendars; }
            set { awardcalendars = value; } 
            
        }

        private string originDestination;
        public string OriginDestination
        {
            get { return originDestination; }
            set { originDestination = string.IsNullOrEmpty(value) ? string.Empty : value; }
        }

        private string awardSubTitleText;

        public string AwardSubTitleText
        {
            get { return awardSubTitleText; }
            set { awardSubTitleText = string.IsNullOrEmpty(value) ? string.Empty : value; }
        }

        private string dateRangeDisplay ;

        public string DateRangeDisplay
        {
            get { return dateRangeDisplay; }
            set { dateRangeDisplay = string.IsNullOrEmpty(value) ? string.Empty : value; }
        }

        private bool isLeftArrowDisabled;

        public bool IsLeftArrowDisabled
        {
            get { return isLeftArrowDisabled; }
            set { isLeftArrowDisabled = value; }
        }

        private bool isRightArrowDisabled;

        public bool IsRightArrowDisabled
        {
            get { return isRightArrowDisabled; }
            set { isRightArrowDisabled = value; }
        }
        
    }
}
