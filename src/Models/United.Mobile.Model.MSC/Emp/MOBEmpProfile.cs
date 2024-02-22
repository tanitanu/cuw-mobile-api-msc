using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpProfile
    {
        private string mileagePlusNumber = string.Empty;
        private string empId;
        private int profileId;
        private MOBName ownerName;
        private List<MOBEmpTraveler> travelers;
        private bool isOneTimeProfileUpdateSuccess;
        private bool isProfileOwnerTSAFlagON;
        private List<MOBTypeOption> disclaimerList = null;

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

        public string EmpId
        {
            get
            {
                return this.empId;
            }
            set
            {
                this.empId = value;
            }
        }

        [XmlIgnore]
        public int ProfileId
        {
            get
            {
                return this.profileId;
            }
            set
            {
                this.profileId = value;
            }
        }

        public MOBName OwnerName
        {
            get
            {
                return this.ownerName;
            }
            set
            {
                this.ownerName = value;
            }
        }

        public List<MOBEmpTraveler> Travelers
        {
            get
            {
                return this.travelers;
            }
            set
            {
                this.travelers = value;
            }
        }

        public bool IsOneTimeProfileUpdateSuccess
        {
            get
            {
                return this.isOneTimeProfileUpdateSuccess;
            }
            set
            {
                this.isOneTimeProfileUpdateSuccess = value;
            }
        }

        public bool IsProfileOwnerTSAFlagON
        {
            get
            {
                return this.isProfileOwnerTSAFlagON;
            }
            set
            {
                this.isProfileOwnerTSAFlagON = value;
            }
        }

        public List<MOBTypeOption> DisclaimerList
        {
            get { return disclaimerList; }
            set { disclaimerList = value; }
        }
    }
}
