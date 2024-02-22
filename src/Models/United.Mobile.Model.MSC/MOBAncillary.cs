using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Accelerators;
using United.Definition.Pcu;
using United.Definition.Shopping;
using United.Mobile.Model.ManageRes;

namespace United.Definition
{
    [Serializable()]
    public class MOBAncillary
    {
        private MOBAccelerators awardAccelerators;
        private MOBPremiumCabinUpgrade premiumCabinUpgrade;
        private MOBPriorityBoarding priorityBoarding;
        private MOBPlacePass placePass;
        private MOBTravelOptionsBundle travelOptionsBundle;
        private MOBBasicEconomyBuyOut basicEconomyBuyOut;

        public MOBPlacePass PlacePass
        {
            get { return placePass; }
            set { placePass = value; }
        }

        public MOBAccelerators AwardAccelerators
        {
            get { return awardAccelerators; }
            set { awardAccelerators = value; }
        }

        public MOBPremiumCabinUpgrade PremiumCabinUpgrade
        {
            get { return premiumCabinUpgrade; }
            set { premiumCabinUpgrade = value; }
        }

        public MOBPriorityBoarding PriorityBoarding
        {
            get { return priorityBoarding; }
            set { priorityBoarding = value; }
        }

        public MOBTravelOptionsBundle TravelOptionsBundle
        {
            get { return travelOptionsBundle; }
            set { travelOptionsBundle = value; }
        }

        public MOBBasicEconomyBuyOut BasicEconomyBuyOut
        {
            get { return basicEconomyBuyOut; }
            set { basicEconomyBuyOut = value; }
        }
    }    
}
