using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Definition.FormofPayment
{
    [Serializable()]
    public class MOBInFlightPurchaseBaseRequest : MOBRequest
    {
        private string flow;
        private string pnr = string.Empty;
        private string pnrType;
        private List<MOBInflightPurchaseFlightSegments> flightSegments;

        public string Flow
        {
            get { return this.flow; }
            set { this.flow = value; }
        }

        public string Pnr
        {
            get
            {
                return this.pnr;
            }
            set
            {
                this.pnr = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string PnrType
        {
            get
            {
                return this.pnrType;
            }
            set
            {
                this.pnrType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public List<MOBInflightPurchaseFlightSegments> FlightSegments
        {
            get { return this.flightSegments; }
            set { this.flightSegments = value; }
        }
    }

    [Serializable()]
    public class MOBInFlightPurchaseRequest : MOBInFlightPurchaseBaseRequest
    {
        private string mileagePlusNumber = string.Empty;
        private string hashValue = string.Empty;
        private bool isLoggedIn;
        private string customerId = string.Empty;
        private string  allPaxInTrans;
        private string allPaxNamesInPnr;

        public string CustomerId
        {
            get
            {
                return this.customerId;
            }
            set
            {
                this.customerId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public string HashValue
        {
            get
            {
                return this.hashValue;
            }
            set
            {
                this.hashValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public bool IsLoggedIn
        {
            get { return this.isLoggedIn; }
            set { this.isLoggedIn = value; }
        }
        public string AllPaxInTrans
        {
            get { return allPaxInTrans; }
            set { allPaxInTrans = value; }
        }
        public string AllPaxNamesInPnr
        {
            get { return allPaxNamesInPnr; }
            set { allPaxNamesInPnr = value; }
        }
    }
}
