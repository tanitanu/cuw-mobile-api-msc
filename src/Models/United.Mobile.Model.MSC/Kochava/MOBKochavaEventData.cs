using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Kochava
{
    public class MOBKochavaEventData : MOBKochavaData
    {
        private string device_ua;
        private string event_name;
        private string currency;
        private MOBEvent_Data event_data;
        /// <summary>
        ///Device_UA
        /// </summary>       
        public string Device_ua
        {
            get { return device_ua; }
            set { device_ua = value; }
        }

        /// <summary>
        /// Event_Name
        /// </summary>       
        public string Event_Name
        {
            get { return event_name; }
            set { event_name = value; }
        }

        /// <summary>
        /// Currency
        /// </summary>
        public string Currency
        {
            get { return currency; }
            set { currency = value; }
        }

        /// <summary>
        ///Event Data
        /// </summary>        
        public MOBEvent_Data Event_Data
        {
            get { return event_data; }
            set { event_data = value; }
        }
    }
}

