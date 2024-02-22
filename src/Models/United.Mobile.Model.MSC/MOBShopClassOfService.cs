using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBShopClassOfService
    {
        private string fareClass = string.Empty;
        private string seatAvailable = string.Empty;

        public string FareClass
        {
            get
            {
                return this.fareClass;
            }
            set
            {
                this.fareClass = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SeatAvailable
        {
            get
            {
                return this.seatAvailable;
            }
            set
            {
                this.seatAvailable = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
