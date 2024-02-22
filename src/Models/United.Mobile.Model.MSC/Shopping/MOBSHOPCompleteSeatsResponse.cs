using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPCompleteSeatsResponse : MOBResponse
    {
        public MOBSHOPCompleteSeatsResponse()
            : base()
        {
        }
        private string sessionId = string.Empty;
        private MOBSHOPCompleteSeatsRequest flightCompleteSeatsRequest;
        private List<MOBCPTraveler> travelers;
        //private FlightAddTripResponse flightAddTripResponse;
        private List<MOBCreditCard> creditCards;
        private List<MOBSeat> seats;
        private List<MOBSeatPrice> seatPrices;
        private List<MOBAddress> profileOwnerAddresses;
        private List<MOBEmail> emails;
        private List<string> termsAndConditions;
        private MOBSHOPReservation reservation;
        private List<MOBTypeOption> hazMat;
        private List<MOBTypeOption> disclaimer;
        private string contractOfCarriage = string.Empty;

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

        public MOBSHOPCompleteSeatsRequest FlightCompleteSeatsRequest
        {
            get { return this.flightCompleteSeatsRequest; }
            set { this.flightCompleteSeatsRequest = value; }
        }

        public List<MOBCPTraveler> Travelers
        {
            get { return this.travelers; }
            set { this.travelers = value; }
        }

        //public FlightAddTripResponse FlightAddTripResponse
        //{
        //    get { return this.flightAddTripResponse; }
        //    set { this.flightAddTripResponse = value; }
        //}

        public List<MOBCreditCard> CreditCards
        {
            get { return this.creditCards; }
            set { this.creditCards = value; }
        }

        public List<MOBSeat> Seats
        {
            get { return this.seats; }
            set { this.seats = value; }
        }

        public List<MOBSeatPrice> SeatPrices
        {
            get { return this.seatPrices; }
            set { this.seatPrices = value; }
        }

        public List<MOBAddress> ProfileOwnerAddresses
        {
            get { return this.profileOwnerAddresses; }
            set { this.profileOwnerAddresses = value; }
        }

        public List<MOBEmail> Emails
        {
            get { return this.emails; }
            set { this.emails = value; }
        }

        public List<string> TermsAndConditions
        {
            get { return this.termsAndConditions; }
            set { this.termsAndConditions = value; }
        }

        public MOBSHOPReservation Reservation
        {
            get { return this.reservation; }
            set { this.reservation = value; }
        }

        public List<MOBTypeOption> HazMat
        {
            get { return this.hazMat; }
            set { this.hazMat = value; }
        }

        private string footerMessage = null;

        public string FooterMessage
        {
            get { return footerMessage; }
            set { footerMessage = value; }
        }

        public List<MOBTypeOption> Disclaimer
        {
            get
            {
                return this.disclaimer;
            }
            set
            {
                this.disclaimer = value;
            }
        }

        public string ContractOfCarriage
        {
            get
            {
                return this.contractOfCarriage;
            }
            set
            {
                this.contractOfCarriage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
