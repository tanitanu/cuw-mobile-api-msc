using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPSelectSeatsResponse : MOBResponse
    {
        private MOBSHOPSelectSeatsRequest flightTravelerSeatsRequest;
        private List<MOBSeat> seats;
        private List<MOBSeatMap> seatMap;
        private List<MOBTypeOption> exitAdvisory;
        private string sessionId = string.Empty;
        private string epaMessageTitle = string.Empty;
        private string epaMessage = string.Empty;

        public string EPAMessageTitle
        {
            get { return this.epaMessageTitle; }
            set { this.epaMessageTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string EPAMessage
        {
            get { return this.epaMessage; }
            set { this.epaMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
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
        public MOBSHOPSelectSeatsRequest FlightTravelerSeatsRequest
        {
            get { return this.flightTravelerSeatsRequest; }
            set { this.flightTravelerSeatsRequest = value; }
        }

        public List<MOBSeat> Seats
        {
            get { return this.seats; }
            set { seats = value; }
        }

        public List<MOBSeatMap> SeatMap
        {
            get { return this.seatMap; }
            set { this.seatMap = value; }
        }

        public List<MOBTypeOption> ExitAdvisory
        {
            get { return this.exitAdvisory; }
            set { this.exitAdvisory = value; }
        }

    }
}
