using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
  
    [Serializable]
    public class MOBEmpPassBalance 
    {
        private List<MOBEmpPassSummaryAllotment> empPassSummaryAllotments;

        public List<MOBEmpPassSummaryAllotment> EmpPassSummaryAllotments
        {
            get { return empPassSummaryAllotments; }
            set { empPassSummaryAllotments = value; }
        }
    }

    [Serializable]
    public class MOBEmpPassSummaryAllotment
    {
        private string type;
        private string description;
        //private DateTime effectiveDate;
        //private DateTime expirationDate;
        private int count;
        private string status;
        private string travelerType;
        private string travelerTypeDescription;
        private List<MOBEmpPassDetailAllotment> empPassDetailAllotment;

        public string Type
        {
            get 
            { 
                return type; 
            }
            set 
            {
                type = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ; 
            }
        }

        public string Description
        {
            get 
            { 
                return description;
            }
            set 
            {
                description = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ; 
            }
        }

        /*public DateTime EffectiveDate
        {
            get 
            { 
                return effectiveDate; 
            }
            set 
            { 
                effectiveDate = value; 
            }
        }

        public DateTime ExpirationDate
        {
            get 
            { 
                return expirationDate; 
            }
            set 
            { 
                expirationDate = value; 
            }
        }*/

        public int Count
        {
            get 
            { 
                return count; 
            }
            set 
            { 
                count = value; 
            }
        }

        public string Status
        {
            get 
            { 
                return status; 
            }
            set 
            {
                status = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ; 
            }
        }

        public string TravelerType
        {
            get 
            { 
                return travelerType; 
            }
            set 
            {
                travelerType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ; 
            }
        }
        public string TravelerTypeDescription
        {
            get 
            { 
                return travelerTypeDescription; 
            }
            set 
            {
                travelerTypeDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ; 
            }
        }


        public List<MOBEmpPassDetailAllotment> EmpPassDetailAllotment
        {
            get 
            {
                return empPassDetailAllotment; 
            }
            set 
            {
                empPassDetailAllotment = value; 
            }
        }
    }

    [Serializable]
    public class MOBEmpPassDetailAllotment
    {

        private string effectiveDate;
        private string expirationDate;
        public int flown;
        public int pending;
        public int remaining;
        public string programYear;
        public int totalCount;

        public string EffectiveDate
        {
            get { return effectiveDate; }
            set { effectiveDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string ExpirationDate
        {
            get { return expirationDate; }
            set { expirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public int Flown
        {
            get { return flown; }
            set { flown = value; }
        }

        public int Pending
        {
            get { return pending; }
            set { pending = value; }
        }

        public int Remaining
        {
            get { return remaining; }
            set { remaining = value; }
        }

        public string ProgramYear
        {
            get { return programYear; }
            set { programYear = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();  }
        }

        public int TotalCount
        {
            get { return totalCount; }
            set { totalCount = value; }
        }
    }
}
