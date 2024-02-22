using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Emp.Shopping;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpFlightSearchRequest: MOBEmpRequest
    {
        #region Constants and Fields
        MOBEmpSHOPShopRequest mobShopRequest;
        private string carrierCode;
        private bool isChangeSegment;
        private string passType;
        private string qualifiedEmergency;
        private string resultType;
        private string returnDate;
        #endregion //Constants and Fields

        #region PublicVariables
        public MOBEmpSHOPShopRequest MobShopRequest
        {
            get
            {
                return this.mobShopRequest;
            }
            set
            {
                this.mobShopRequest = value;
            }
        }
        public string CarrierCode
        {
            get
            {
                return this.carrierCode;
            }
            set
            {
                this.carrierCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public bool IsChangeSegment
        {
            get
            {
                return this.isChangeSegment;
            }

            set
            {
                this.isChangeSegment = value;
            }
        }
        public string PassType
        {
            get
            {
                return this.passType;
            }

            set
            {
                this.passType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string QualifiedEmergency
        {
            get
            {
                return this.qualifiedEmergency;
            }

            set
            {
                this.qualifiedEmergency = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string ResultType
        {
            get
            {
                return this.resultType;
            }

            set
            {
                this.resultType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string ReturnDate
        {
            get
            {
                return this.returnDate;
            }

            set
            {
                this.returnDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        #endregion //PublicVariables
    }
}