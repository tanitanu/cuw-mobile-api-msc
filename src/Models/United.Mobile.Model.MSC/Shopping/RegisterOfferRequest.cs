using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Persist.Definition.Shopping
{
    [Serializable]
    public class RegisterOfferRequest : IPersist
    {

        private string objectName = "United.Persist.Definition.Shopping.RegisterOfferRequest";
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

        public United.Definition.Shopping.MOBSHOPRegisterOfferRequest Request { get; set; }
    }
}
