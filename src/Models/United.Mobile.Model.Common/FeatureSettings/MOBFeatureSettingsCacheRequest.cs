﻿using System;
using System.Collections.Generic;
using System.Text;
using United.Definition;

namespace United.Mobile.Model.Common
{
    public class MOBFeatureSettingsCacheRequest:MOBRequest
    {
        private string ipAddressList;

        public string IpAddressList
        {
            get { return ipAddressList; }
            set { ipAddressList = value; }
        }
        private string serviceName;

        public string ServiceName
        {
            get { return serviceName; }
            set { serviceName = value; }
        }
        private string token;

        public string Token
        {
            get { return token; }
            set { token = value; }
        }

    }
}
