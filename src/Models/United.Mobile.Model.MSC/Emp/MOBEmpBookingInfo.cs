using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Emp.Common;
using United.Mobile.Model.Common;
namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpBookingInfo : MOBRequest
    {
        private int numberOfTrips;
        private List<MOBEmpBookingTrip> bookingTrips;
        private List<MOBEmpBookingPassenger> bookingPassengers;
        private string workPhone;
        private string homePhone;
        private string deliveryType;
        private string deliveryValue;
        private MOBEmpName name;
        private string cardType;
        private string cardTypeName;
        private string cardNumber;
        private string cid;
        private string expMonth;
        private string expYear;
        private string passType;
        private string qualifiedEmergency;
        private string emergencyNature;
        private bool payByCreditCard;
        private decimal totalCost;
        private bool isChangeSegment;
        private MOBEmpTravelingWith travelingWith;
        private string bookingConfirmationNumber;

        private string sessionId;

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        public string BookingConfirmationNumber
        {
            get { return bookingConfirmationNumber; }
            set { bookingConfirmationNumber = value; }
        }

        public int NumberOfTrips
        {
            get { return numberOfTrips; }
            set { numberOfTrips = value; }
        }
        
        public List<MOBEmpBookingTrip> BookingTrips
        {
            get { return bookingTrips; }
            set { bookingTrips = value; }
        }

        public List<MOBEmpBookingPassenger> BookingPassengers
        {
            get { return bookingPassengers; }
            set { bookingPassengers = value; }
        }

        public string WorkPhone
        {
            get { return workPhone; }
            set { workPhone = value; }
        }

        public string HomePhone
        {
            get { return homePhone; }
            set { homePhone = value; }
        }

        public string DeliveryType
        {
            get { return deliveryType; }
            set { deliveryType = value; }
        }

        public string DeliveryValue
        {
            get { return deliveryValue; }
            set { deliveryValue = value; }
        }
        public MOBEmpName Name
        {
            get { return name; }
            set { name = value; }
        }
        public string CardType
        {
            get { return cardType; }
            set { cardType = value; }
        }

        public string CardTypeName
        {
            get { return cardTypeName; }
            set { cardTypeName = value; }
        }

        public string CardNumber
        {
            get { return cardNumber; }
            set { cardNumber = value; }
        }

        public string CID
        {
            get { return cid; }
            set { cid = value; }
        }

        public string ExpMonth
        {
            get { return expMonth; }
            set { expMonth = value; }
        }

        public string ExpYear
        {
            get { return expYear; }
            set { expYear = value; }
        }

        public string PassType
        {
            get { return passType; }
            set { passType = value; }
        }

        public string QualifiedEmergency
        {
            get { return qualifiedEmergency; }
            set { qualifiedEmergency = value; }
        }

        public string EmergencyNature
        {
            get { return emergencyNature; }
            set { emergencyNature = value; }
        }

        public bool PayByCreditCard
        {
            get { return payByCreditCard; }
            set { payByCreditCard = value; }
        }
        public decimal TotalCost
        {
            get { return totalCost; }
            set { totalCost = value; }
        }

        public bool IsChangeSegment
        {
            get { return isChangeSegment; }
            set { isChangeSegment = value; }
        }

        public MOBEmpTravelingWith TravelingWith
        {
            get { return travelingWith; }
            set { travelingWith = value; }
        }
    }
}
