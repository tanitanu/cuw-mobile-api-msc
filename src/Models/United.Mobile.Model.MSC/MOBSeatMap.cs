using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Definition
{
    [Serializable()] 
    public class MOBSeatMap
    {
        private int flightNumber;
        private string flightDateTime = string.Empty;
        private MOBAirport departure;
        private MOBAirport arrival;
        private string codeshareFlightNumber = string.Empty;
        private string seatLegendId = string.Empty;
        private List<MOBCabin> cabins = new List<MOBCabin>();
        private string legId = string.Empty;
        private string fleetType = string.Empty;
        private bool seatMapAvailabe;
        private string eplusPromotionMessage = string.Empty;
        private bool suppressLMX;
        public string showInfoMessageOnSeatMap;
        private bool isOaSeatMap;
        private string operatedByText;
        private bool isReadOnlySeatMap =false;
        private string oASeatMapBannerMessage = string.Empty;
        private List<MOBItem> captions;

        public string ShowInfoMessageOnSeatMap
        {
            get { return showInfoMessageOnSeatMap; }
            set { showInfoMessageOnSeatMap = value; }
        }

        public bool SuppressLMX
        {
            get { return suppressLMX; }
            set { suppressLMX = value; }
        }

        public string EplusPromotionMessage
        {
            get
            {
                return this.eplusPromotionMessage;
            }
            set
            {
                this.eplusPromotionMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int FlightNumber
        {
            get
            {
                return this.flightNumber;
            }
            set
            {
                this.flightNumber = value;
            }
        }

        public string FlightDateTime
        {
            get
            {
                return this.flightDateTime;
            }
            set
            {
                this.flightDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBAirport Departure
        {
            get { return departure; }
            set { departure = value; }
        }

        public MOBAirport Arrival
        {
            get { return arrival; }
            set { arrival = value; }
        }

        public string CodeshareFlightNumber
        {
            get
            {
                return this.codeshareFlightNumber;
            }
            set
            {
                this.codeshareFlightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string SeatLegendId
        {
            get
            {
                return this.seatLegendId;
            }
            set
            {
                this.seatLegendId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBCabin> Cabins
        {
            get { return cabins; }
            set { cabins = value; }
        }

        public string LegId
        {
            get
            {
                return this.legId;
            }
            set
            {
                this.legId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FleetType
        {
            get
            {
                return this.fleetType;
            }
            set
            {
                this.fleetType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool SeatMapAvailabe
        {
            get { return seatMapAvailabe; }
            set { seatMapAvailabe = value; }
        }

        public bool IsOaSeatMap
        {
            get { return isOaSeatMap; }
            set { isOaSeatMap = value; }
        }
      
        public string OperatedByText
        {
            get { return operatedByText; }
            set { operatedByText = value; }
        }
        public bool IsReadOnlySeatMap
        {
            get { return isReadOnlySeatMap; }
            set { isReadOnlySeatMap = value; }
            }
        public string OASeatMapBannerMessage
        {
            get { return oASeatMapBannerMessage; }
            set { oASeatMapBannerMessage = value; }
        }

        public List<MOBItem> Captions
        {
            get { return captions; }
            set { captions = value; }
        }
    }
}
