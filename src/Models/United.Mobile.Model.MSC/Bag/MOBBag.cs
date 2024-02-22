using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBBag
    {
        private List<United.Definition.Bag.MOBBagHistory> bagHistories;
        private MOBBagItinerary bagItinerary;
        private MOBBagTag bagTag;
        private MOBPassenger passenger;
        private bool bagRerouted;

        public List<MOBBagHistory> BagHistories
        {
            get
            {
                return this.bagHistories;
            }
            set
            {
                this.bagHistories = value;
            }
        }

        public MOBBagItinerary BagItinerary
        {
            get
            {
                return this.bagItinerary;
            }
            set
            {
                this.bagItinerary = value;
            }
        }

        public MOBBagTag BagTag
        {
            get
            {
                return this.bagTag;
            }
            set
            {
                this.bagTag = value;
            }
        }

        public MOBPassenger Passenger
        {
            get
            {
                return this.passenger;
            }
            set
            {
                this.passenger = value;
            }
        }

        public bool BagRerouted
        {
            get
            {
                return this.bagRerouted;
            }
            set
            {
                this.bagRerouted = value;
            }
        }
    }
}
