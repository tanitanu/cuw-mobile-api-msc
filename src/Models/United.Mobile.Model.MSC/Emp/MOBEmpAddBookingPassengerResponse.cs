using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpAddBookingPassengerResponse: MOBResponse
    {
        private List<MOBEmpBookingItinerary> bookingItineraries;
        private string duplicateBooking;
        private MOBEmployeeProfileExtended employeeProfileExtended;
        private bool mustPrePay;
        private bool isSecureFlightNeeded;
        private bool isTCDMessageNeeded;
        private string totalCost;

        public List<MOBEmpBookingItinerary> BookingItineraries
        {
            get
            {
                return bookingItineraries;
            }
            set
            {
                this.bookingItineraries = value;
            }
        }
        public string DuplicateBooking
        {
            get
            {
                return duplicateBooking;
            }
            set
            {
                this.duplicateBooking = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public MOBEmployeeProfileExtended EmployeeProfileExtended
        {
            get
            {
                return employeeProfileExtended;
            }
            set
            {
                this.employeeProfileExtended = value;
            }
        }

        public bool MustPrePay 
        {
            get
            {
                return mustPrePay;
            }
            set
            {
                this.mustPrePay = value;
            }
        }
        public bool IsSecureFlightNeeded 
        {
            get
            {
                return isSecureFlightNeeded;
            }
            set
            {
                this.isSecureFlightNeeded = value;
            }
        }
        public bool IsTCDMessageNeeded 
        {
            get
            {
                return isTCDMessageNeeded;
            }
            set
            {
                this.isTCDMessageNeeded = value;
            }
        }
        public string TotalCost 
        {
            get
            {
                return totalCost;
            }
            set
            {
                this.totalCost = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

    }
}
