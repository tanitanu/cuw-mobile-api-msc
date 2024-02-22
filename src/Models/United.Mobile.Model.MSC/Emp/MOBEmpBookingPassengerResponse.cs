using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpBookingPassengerResponse : MOBResponse
    {

        private MOBEmpBookingPassengerRequest empBookingPassengerRequest;

        private List<MOBEmpBookingPassengerExtended> passengerList;

        private bool isBuddiesAllowed;

        private bool isCustomize;

        private bool isOnlyFamilyBuddies;

        private bool isWorkingCrewMember;

        public MOBEmpBookingPassengerRequest EmpBookingPassengerRequest
        {
            get
            {
                return empBookingPassengerRequest;
            }
            set
            {
                empBookingPassengerRequest = value;
            }
        }

        public List<MOBEmpBookingPassengerExtended> PassengerList
        {
            get
            {
                return passengerList;
            }
            set
            {
                passengerList = value;
            }
        }

        public bool IsBuddiesAllowed
        {
            get
            {
                return isBuddiesAllowed;
            }
            set
            {
                isBuddiesAllowed = value;
            }
        }

        public bool IsCustomize
        {
            get
            {
                return isCustomize;
            }
            set
            {
                isCustomize = value;
            }
        }

        public bool IsOnlyFamilyBuddies
        {
            get
            {
                return isOnlyFamilyBuddies;
            }
            set
            {
                isOnlyFamilyBuddies = value;
            }
        }


        public bool IsWorkingCrewMember
        {
            get
            {
                return isWorkingCrewMember;
            }
            set
            {
                isWorkingCrewMember = value;
            }
        }
    }
}
