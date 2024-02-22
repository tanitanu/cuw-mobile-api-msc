using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Booking;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBSeatChangeSelectResponse : MOBResponse
    {
        private MOBSeatChangeSelectRequest request;
        private string sessionId = string.Empty;
        private List<MOBSeat> seats;
        private List<MOBSeatMap> seatMap;
        private List<MOBTypeOption> exitAdvisory;
        private List<MOBBKTraveler> bookingTravlerInfo;
        private List<MOBBKTrip> selectedTrips;


        public MOBSeatChangeSelectRequest Request
        {
            get { return this.request; }
            set { this.request = value; }
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

        public List<MOBBKTraveler> BookingTravelerInfo
        {
            get { return this.bookingTravlerInfo; }
            set { this.bookingTravlerInfo = value; }
        }

        public List<MOBBKTrip> SelectedTrips
        {
            get { return this.selectedTrips; }
            set { this.selectedTrips = value; }
        }
    }
}
