using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Emp.Common;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpBookingPassenger
    {
        private string paxID;
        private string index;
        private MOBEmpName name;
        private MOBTypeOption cabin;
        private MOBEmpPassType passType;
        private List<int> segmentIndexes;
        private MOBEmpRelationship relationshipObject;
        private int age;
        private MOBEmpPaxPrice paxPrice;
        private MOBEmpBookingLapChild lapChild;
        private int sortOrder;
        private MOBEmpSpecialService specialService;
        private bool isPrimaryFriend;
        private MOBEmpTCDInfo tcdInfo;
        private int travelerNumber;

        public MOBEmpTCDInfo TcdInfo
        {
            get
            {
                return tcdInfo;
            }
            set
            {
                tcdInfo = value;
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
                lapChild = value;
            }
        }


        public string PassengerId
        {
            get
            {
                return paxID;
            }
            set
            {
                paxID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
                index = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public int Age
        {
            get
            {
                return age;
            }
            set
            {
                age = value;
            }

        }

        public MOBEmpName Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public MOBTypeOption Cabin
        {
            get
            {
                return cabin;
            }
            set
            {
                cabin = value;
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
                passType = value;
            }
        }

        
        public List<int> SegmentIndexes
        {
            get
            {
                return segmentIndexes;
            }
            set
            {
                segmentIndexes = value;
            }
        }

        public MOBEmpSpecialService SpecialService
        {
            get
            {
                return this.specialService;
            }
            set
            {
                this.specialService = value;
            }

        }

        public MOBEmpRelationship Relationship
        {
            get
            {
                return this.relationshipObject;
            }
            set
            {
                this.relationshipObject = value;
            }
        }

        public MOBEmpPaxPrice PaxPrice
        {
            get { return paxPrice; }
            set { paxPrice = value; }
        }

        public bool IsPrimaryFriend
        {
            get { return isPrimaryFriend; }
            set { isPrimaryFriend = value; }
        }

        public int SortOrder
        {
            get
            {
                return this.sortOrder;
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
                return this.travelerNumber;
            }
            set
            {
                this.travelerNumber = value;
            }
        }

    }
}
