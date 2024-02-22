using United.Mobile.Model.Common;
using United.Service.Presentation.ProductResponseModel;

namespace United.Persist.Definition.Merchandizing
{
    public class GetVendorOffers : ProductOffer, IPersist
    {
        public GetVendorOffers() { }

        #region IPersist Members
        private string objectName = "United.Persist.Definition.Merchandizing.GetVendorOffers";

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
    }
}
