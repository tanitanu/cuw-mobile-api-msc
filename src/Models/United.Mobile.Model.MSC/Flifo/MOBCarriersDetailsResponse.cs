using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace United.Definition.Flifo
{
    [Serializable]
    public class MOBCarriersDetailsResponse
    {
        public MOBCarriersDetailsResponse()
        { }
        
        private List<MOBCarriersDetails> carriersDetails;
        public List<MOBCarriersDetails> CarriersDetails
        {
            get { return carriersDetails; }
            set { carriersDetails = value; }
        }

    }
    [Serializable]
    public class MOBCarriersDetails
    {
        private string carrierCode;
        private string carrierShortName;
        private string carrierFullName;
        private string carrierImageSrc;
        private string carrierImageName;

        public string CarrierCode
        {
            get { return carrierCode; }
            set { carrierCode = value; }
        }
        public string CarrierShortName
        {
            get { return carrierShortName; }
            set { carrierShortName = value; }
        }

        public string CarrierFullName
        {
            get { return carrierFullName; }
            set { carrierFullName = value; }
        }
        public string CarrierImageSrc
        {
            get { return carrierImageSrc; }
            set { carrierImageSrc = value; }
        }

        public string CarrierImageName
        {
            get { return carrierImageName; }
            set { carrierImageName = value; }
        }

    }
}
