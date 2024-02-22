﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmployeeProfileExtended
    {
        private string email; 
        private string faxNumber; 
        private string homePhone; 
        private string workPhone; 

        public string Email 
        {
            get
            {
                return email;
            }
            set
            {
                this.email = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string FaxNumber 
        {
            get
            {
                return faxNumber;
            }
            set
            {
                this.faxNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string HomePhone
        {
            get
            {
                return homePhone;
            }
            set
            {
                this.homePhone = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string WorkPhone
        {
            get
            {
                return workPhone;
            }
            set
            {
                this.workPhone = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
