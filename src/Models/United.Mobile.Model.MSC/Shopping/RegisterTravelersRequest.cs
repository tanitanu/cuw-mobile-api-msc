using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.ReservationModel;


namespace United.Persist.Definition.Shopping
{
    [Serializable()]
    public class RegisterTravelersRequest : IPersist
    {
        #region IPersist Members

        private string objectName = "United.Persist.Definition.Shopping.RegisterTravelersRequest";
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
        public string CartId { get; set; }
        public Collection<Traveler> Travelers { get; set; }
        public Collection<Telephone> Telephones { get; set; }
    }
}
