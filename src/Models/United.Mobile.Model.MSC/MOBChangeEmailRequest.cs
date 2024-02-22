﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBChangeEmailRequest : MOBRequest
    {        
        private string sessionId;
        private MOBEmail mobEmail;
        private string cartId = string.Empty;
        private string flow = string.Empty;

        public MOBEmail MobEmail
        {
            get
            {
                return mobEmail;
            }
            set
            {
                mobEmail = value;
            }
        }
        
        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string CartId
        {
            get { return cartId; }
            set { cartId = value; }
        }
        public string Flow
        {
            get { return flow; }
            set { flow = value; }
        }
    }
}