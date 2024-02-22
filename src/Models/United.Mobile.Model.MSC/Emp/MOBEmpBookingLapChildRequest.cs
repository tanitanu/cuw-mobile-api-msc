﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Emp.Common;
using United.Mobile.Model.Common;
namespace United.Definition.Emp
{
    public class MOBEmpBookingLapChildRequest : MOBRequest
    {
        private MOBEmpName name;
        private string gender;
        private string bday;
        private string redress;
        private string paxId;

        public MOBEmpName Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public string Gender
        {
            get
            {
                return gender;
            }
            set
            {
                gender = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Bday
        {
            get
            {
                return bday;
            }
            set
            {
                bday = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Redress
        {
            get
            {
                return redress;
            }
            set
            {
                redress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PaxId
        {
            get
            {
                return paxId;
            }
            set
            {
                paxId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }


    }
}
