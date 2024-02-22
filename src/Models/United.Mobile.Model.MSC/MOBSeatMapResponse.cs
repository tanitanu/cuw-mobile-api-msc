using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBSeatMapResponse : MOBResponse
    {
        private string marketingCarrierCode;
        private string operatingCarrierCode;
        private string flightNumber = string.Empty;
        private string flightDate = string.Empty;
        private string departureAirportCode = string.Empty;
        private string arrivalAirportCode = string.Empty;
        private United.Definition.MOBSeatMap seatMap;
        private MOBSHOPOnTimePerformance onTimePerformance;
        private string sessionId = string.Empty;
        private MOBSeatMapRequest seatMapRequest;

        public MOBSeatMapResponse()
            : base()
        {
        }

        public MOBSeatMapRequest SeatMapRequest
        {
            get
            {
                return this.seatMapRequest;
            }
            set
            {
                this.seatMapRequest = value;
            }
        }

        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MarketingCarrierCode
        {
            get
            {
                return this.marketingCarrierCode;
            }
            set
            {
                this.marketingCarrierCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string OperatingCarrierCode
        {
            get
            {
                return this.operatingCarrierCode;
            }
            set
            {
                this.operatingCarrierCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string FlightNumber
        {
            get
            {
                return this.flightNumber;
            }
            set
            {
                this.flightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FlightDate
        {
            get
            {
                return this.flightDate;
            }
            set
            {
                this.flightDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DepartureAirportCode
        {
            get
            {
                return this.departureAirportCode;
            }
            set
            {
                this.departureAirportCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ArrivalAirportCode
        {
            get
            {
                return this.arrivalAirportCode;
            }
            set
            {
                this.arrivalAirportCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public Definition.MOBSeatMap SeatMap
        {
            get
            {
                return this.seatMap;
            }
            set
            {
                this.seatMap = value;
            }
        }

        public MOBSHOPOnTimePerformance OnTimePerformance
        {
            get
            {
                return this.onTimePerformance;
            }
            set
            {
                this.onTimePerformance = value;
            }
        }
    }
}
