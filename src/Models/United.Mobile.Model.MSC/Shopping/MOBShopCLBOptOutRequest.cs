﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBShopCLBOptOutRequest : MOBRequest
    {
        private string sessionId;
        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }
    }
}
