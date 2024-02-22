using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Emp.Common;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpSelectedPassenger
    {

        private MOBTypeOption cabin;
        private string id;
        private string index;
        private MOBEmpBookingLapChild lapChild;
        private MOBEmpPassType passType;
        private MOBEmpTCDInfo empTCDInfo;
        private MOBEmpSSRInfo empSSRInfo;
        private int sortOrder;
        private int travelerNumber;

        public MOBTypeOption Cabin 
        {
            get
            {
                return cabin;
            }
            set
            {
                this.cabin = value; 
            }
        }
        public string Id 
        {
            get
            {
                return id;
            }
            set
            {
                this.id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string Index 
        {
            get
            {
                return index;
            }
            set
            {
                this.index = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public MOBEmpBookingLapChild LapChild
        {
            get
            {
                return lapChild;
            }
            set
            {
                this.lapChild = value;
            }
        }
        public MOBEmpPassType PassType
        {
            get
            {
                return passType;
            }
            set
            {
                this.passType = value;
            }
        }
        public MOBEmpTCDInfo EmpTCDInfo
        {
            get
            {
                return empTCDInfo;
            }
            set
            {
                this.empTCDInfo = value;
            }
        }
        public MOBEmpSSRInfo EmpSSRInfo
        {
            get
            {
                return empSSRInfo;
            }
            set
            {
                this.empSSRInfo = value;
            }
        }
        public int SortOrder
        {
            get
            {
                return sortOrder;
            }
            set
            {
                this.sortOrder = value;
            }
        }
        public int TravelerNumber
        {
            get
            {
                return travelerNumber;
            }
            set
            {
                this.travelerNumber = value;
            }
        }
    }

   
}
