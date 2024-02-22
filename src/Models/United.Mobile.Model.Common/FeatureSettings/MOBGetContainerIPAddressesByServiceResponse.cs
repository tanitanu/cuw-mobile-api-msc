using System;
using System.Collections.Generic;
using System.Text;
using United.Definition;

namespace United.Mobile.Model.Common
{
    public class MOBGetContainerIPAddressesByServiceResponse : MOBResponse
    {
        private List<MOBContainerIPAddressDetails> ipAddresses;

        public List<MOBContainerIPAddressDetails> IpAddresses
        {
            get { return ipAddresses; }
            set { ipAddresses = value; }
        }

    }
}
