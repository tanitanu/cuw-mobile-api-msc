using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.TouchlessPayments;
using United.Mobile.Model.Common;

namespace United.Persist.Definition.InflightPurchase
{
    public class ProfileCreditCardResponse : IPersist
    {
        #region IPersist Members

        private string objectName = "United.Persist.Definition.InflightPurchase.ProfileCreditCardResponse";
        public string ObjectName
        {
            get
            {
                return this.objectName;
            }
            set
            {
                this.objectName = value;
            }
        }

        #endregion

        public string SessionId { get; set; }

        public string MileagePlusNumber { get; set; }

        public CreditCard PrimaryCreditCard { get; set; }
    }
}
