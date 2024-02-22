﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Common
{
    [Serializable()]
    public class MOBEmpLMXTraveler
    {
        private string firstName = string.Empty;
        private string lastName = string.Empty;
        private string awardMileTotalFormated = string.Empty;
        private string pqmTotalFormated = string.Empty;
        private string pqsTotalFormated = string.Empty;
        private string pqdTotalFormated = string.Empty;
        private string awardMileTotalShort = string.Empty;
        private string pqmTotalShort = string.Empty;
        private string pqsTotalShort = string.Empty;
        private string pqdTotalShort = string.Empty;
        private bool isMember;
        private string memberLevelText = string.Empty; //Can be United Level, Non Member, or Star Alliance Partner
        private string nonMemberMessage = string.Empty; //Earning miles with Partner or join mileageplus
        private List<MOBEmpLMXRow> rowData;
        private bool hasInelligibleSegment;

        public string FirstName
        {
            get
            {
                return this.firstName;
            }
            set
            {
                this.firstName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
                this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }

        }

        public string FormattedAwardMileTotal
        {
            get
            {
                return this.awardMileTotalFormated;
            }
            set
            {
                this.awardMileTotalFormated = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }

        }

        public string FormattedPQMTotal
        {
            get
            {
                return this.pqmTotalFormated;
            }
            set
            {
                this.pqmTotalFormated = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }

        }

        public string FormattedPQSTotal
        {
            get
            {
                return this.pqsTotalFormated;
            }
            set
            {
                this.pqsTotalFormated = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }

        }

        public string FormattedPQDTotal
        {
            get
            {
                return this.pqdTotalFormated;
            }
            set
            {
                this.pqdTotalFormated = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }

        }

        public string AwardMileTotal
        {
            get
            {
                return this.awardMileTotalShort;
            }
            set
            {
                this.awardMileTotalShort = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }

        }

        public string PQMTotal
        {
            get
            {
                return this.pqmTotalShort;
            }
            set
            {
                this.pqmTotalShort = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }

        }

        public string PQSTotal
        {
            get
            {
                return this.pqsTotalShort;
            }
            set
            {
                this.pqsTotalShort = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }

        }

        public string PQDTotal
        {
            get
            {
                return this.pqdTotalShort;
            }
            set
            {
                this.pqdTotalShort = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }

        }

        public bool IsMPMember
        {
            get
            {
                return this.isMember;
            }
            set
            {
                this.isMember = value;
            }
        }

        public string MPEliteLevelDescription
        {
            get
            {
                return this.memberLevelText;
            }
            set
            {
                this.memberLevelText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }

        }

        public string NonMPMemberMessage
        {
            get
            {
                return this.nonMemberMessage;
            }
            set
            {
                this.nonMemberMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }

        }

        public List<MOBEmpLMXRow> LMXRows
        {
            get
            {
                return this.rowData;
            }
            set
            {
                this.rowData = value;
            }
        }

        public bool HasIneligibleSegment
        {
            get
            {
                return this.hasInelligibleSegment;
            }
            set
            {
                this.hasInelligibleSegment = value;
            }
        }
    }   
}
