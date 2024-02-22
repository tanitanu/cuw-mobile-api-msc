using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;
using United.Mobile.Model.Common;

namespace United.Persist.Definition.Shopping
{
    public class MakeReservation : IPersist
    {
        #region IPersist Members

        private string objectName = "United.Persist.Definition.Shopping.MakeReservation";
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
        public MOBSHOPMakeReservationRequest Request { get; set; }
        public MOBSHOPMakeReservationResponse Response { get; set; }
    }
}
