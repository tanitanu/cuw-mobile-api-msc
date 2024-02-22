using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPAwardCalendar
    {
        private List<MOBSHOPAwardCalendarItem> awardCalendarItems;
        private string productId = string.Empty;
        private string tripId = string.Empty;

        public List<MOBSHOPAwardCalendarItem> AwardCalendarItems
        {
            get
            {
                return this.awardCalendarItems;
            }
            set
            {
                this.awardCalendarItems = value;
            }
        }


        public string ProductId
        {
            get
            {
                return this.productId;
            }
            set
            {
                this.productId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }


        public string TripId
        {
            get
            {
                return this.tripId;
            }
            set
            {
                this.tripId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
