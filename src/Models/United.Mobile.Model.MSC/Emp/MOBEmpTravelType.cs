using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpTravelType
    {
        private bool isTermsAndConditionsAccepted;
        private int numberOfPassengersInJA;
        private string termsAndConditions = string.Empty; 

        private List<MOBEmpTravelTypeItem> empTravelTypes;
        
        
        public List<MOBEmpTravelTypeItem> EmpTravelTypes
        {
            get
            {
                return empTravelTypes;
            }
            set
            {
                this.empTravelTypes = value;
            }
        }

        public bool IsTermsAndConditionsAccepted
        {
            get
            {
                return isTermsAndConditionsAccepted;
            }
            set
            {
                isTermsAndConditionsAccepted = value;
            }
        }

        public int NumberOfPassengersInJA
        {
            get
            {
                return numberOfPassengersInJA;
            }
            set
            {
                numberOfPassengersInJA = value;
            }
        }

        public string TermsAndConditions
        {
            get
            {
                return this.termsAndConditions;
            }
            set
            {
                this.termsAndConditions = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
