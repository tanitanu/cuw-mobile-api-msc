using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpTravelTypeItem
    {
        private bool isEligible;
        private bool isAuthorizationRequired;
        private int numberOfTravelers;
        private string travelType;
        private string travelTypeDescription;
        private string advisory;


        public bool IsEligible
        {
            get
            {
                return isEligible;
            }
            set
            {
                isEligible = value;
            }
        }

        public bool IsAuthorizationRequired
        {
            get
            {
                return isAuthorizationRequired;
            }
            set
            {
                isAuthorizationRequired = value;
            }
        }

        public int NumberOfTravelers
        {
            get
            {
                return numberOfTravelers;
            }
            set
            {
                numberOfTravelers = value;
            }
        }

        public string TravelType
        {
            get
            {
                return travelType;
            }
            set
            {
                travelType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); 
            }
        }

        public string TravelTypeDescription
        {
            get
            {
                return travelTypeDescription;
            }
            set
            {
                travelTypeDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); 
            }
        }

        public string Advisory
        {
            get
            {
                return advisory;
            }
            set
            {
                advisory = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); 
            }
        }
      
    }
}
