using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBBagsDetailsRequest : MOBRequest
    {
        private string bagTagId = string.Empty;
        private string recordLocator = string.Empty;
        private string lastNames = string.Empty;
        private string mileagePlusAccountNumber;

        public string MileagePlusAccountNumber
        {
            get { return mileagePlusAccountNumber; }
            set { mileagePlusAccountNumber = value; }
        }

        public string BagTagId
        {
            get
            {
                return this.bagTagId;
            }
            set
            {
                this.bagTagId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string RecordLocator
        {
            get
            {
                return this.recordLocator;
            }
            set
            {
                this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string LastNames
        {
            get
            {
                return this.lastNames;
            }
            set
            {
                this.lastNames = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }

    [Serializable]
    public class MOBGetBagsForPNRsRequest : MOBRequest
    {
        private List<MOBPNRListForBags> pnrList;

        public List<MOBPNRListForBags> PNRList
        {
            get { return pnrList; }
            set { pnrList = value; }
        }

        private string mileagePlusAccountNumber;

        public string MileagePlusAccountNumber
        {
            get { return mileagePlusAccountNumber; }
            set { mileagePlusAccountNumber = value; }
        }
    }

    [Serializable]
    public class MOBPNRListForBags
    {
        private string recordLocator = string.Empty;
        private string lastName = string.Empty;
        public string RecordLocator
        {
            get
            {
                return this.recordLocator;
            }
            set
            {
                this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string LastName
        {
            get
            {
                return this.lastName;
            }
            set
            {
                this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }


}
