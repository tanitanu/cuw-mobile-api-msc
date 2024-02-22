using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Fitbit
{
    [Serializable]
    public class MileagePlusSignInRequest : MOBRequest
    {
        private string mileagePlusNumber = string.Empty;
        private string password = string.Empty;
        private string hashValue = string.Empty;

        public MileagePlusSignInRequest()
            : base()
        {
        }

        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public string HashValue
        {
            get
            {
                return this.hashValue;
            }
            set
            {
                this.hashValue = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
    }
}
