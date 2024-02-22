using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.PrePaidBags
{
    [Serializable()]
    public class MOBBaggageDetail
    {
        private string tripInfo; // Chicago, IL(ORD) to Denver, CO(DEN) 
        private string flightDate; // Wed. Jan 31, 2018
        private string firstCheckedBagPrice; // $25
        private string secondCheckedBagPrice; // $35
        private string weightPerBag; // 50lbs (23kg)

        public string TripInfo
        {
            get { return tripInfo; }
            set { tripInfo = value; }
        }

        public string FlightDate
        {
            get { return flightDate; }
            set { flightDate = value; }
        }
        public string FirstCheckedBagPrice
        {
            get { return firstCheckedBagPrice; }
            set { firstCheckedBagPrice = value; }
        }
        public string SecondCheckedBagPrice
        {
            get { return secondCheckedBagPrice; }
            set { secondCheckedBagPrice = value; }
        }
        public string WeightPerBag
        {
            get { return weightPerBag; }
            set { weightPerBag = value; }
        }


    }
}
