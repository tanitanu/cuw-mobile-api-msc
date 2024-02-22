﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBEmail
    {
        private string key = string.Empty;
        private MOBChannel channel;
        private string emailAddress = string.Empty;
        private bool isPrivate;
        private bool isDefault;
        private bool isPrimary;
        private bool isDayOfTravel;

        public string Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBChannel Channel
        {
            get
            {
                return this.channel;
            }
            set
            {
                this.channel = value;
            }
        }

        public string EmailAddress
        {
            get
            {
                return this.emailAddress;
            }
            set
            {
                this.emailAddress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsPrivate
        {
            get
            {
                return this.isPrivate;
            }
            set
            {
                this.isPrivate = value;
            }
        }

        public bool IsDefault
        {
            get
            {
                return this.isDefault;
            }
            set
            {
                this.isDefault = value;
            }
        }

        public bool IsPrimary
        {
            get
            {
                return this.isPrimary;
            }
            set
            {
                this.isPrimary = value;
            }
        }
        public bool IsDayOfTravel
        {
            get
            {
                return this.isDayOfTravel;
            }
            set
            {
                this.isDayOfTravel = value;
            }
        }
    }
}
