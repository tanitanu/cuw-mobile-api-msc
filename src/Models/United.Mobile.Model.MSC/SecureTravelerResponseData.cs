

using System.Collections.Generic;


namespace United.Definition.CSLModels.CustomerProfile
{
    public class SecureTravelerResponseData
    {
        public SecureTraveler SecureTraveler
        {
            get;
            set;
        }

        public List<SupplementaryTravelDocsDataMembers> SupplementaryTravelInfos
        {
            get;
            set;
        }
    }

}

