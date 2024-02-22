using System;
using System.Collections.Generic;

namespace United.Definition
{
    [Serializable()]
    public class MOBWalletPNRResponse
    {
        private List<MOBWalletPNR> pnrs;
        private MOBLocationEvent locationEvent;
        private List<MOBGeoFence> geoFences;
        private bool gotException4GetPNRSbyMPCSLcallDoNotDropExistingPnrsInWallet = false;

        public List<MOBWalletPNR> PNRs
        {
            get
            {
                return this.pnrs;
            }
            set
            {
                this.pnrs = value;
            }
        }

        public MOBLocationEvent LocationEvent
        {
            get
            {
                return this.locationEvent;
            }
            set
            {
                this.locationEvent = value;
            }
        }
        
        public List<MOBGeoFence> GeoFences
        {
            get { return geoFences; }
            set { geoFences = value; }
        }
        public bool GotException4GetPNRSbyMPCSLcallDoNotDropExistingPnrsInWallet
        {
            get { return gotException4GetPNRSbyMPCSLcallDoNotDropExistingPnrsInWallet; }
            set { gotException4GetPNRSbyMPCSLcallDoNotDropExistingPnrsInWallet = value; }
        }
    }
}
