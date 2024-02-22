using United.Mobile.Model.Common;
using United.Service.Presentation.ProductResponseModel;

namespace United.Persist.Definition.Merchandizing
{
    public class GetOffers : ProductOffer, IPersist
    {
        public GetOffers() { }

        #region IPersist Members
        private string objectName = "United.Persist.Definition.Merchandizing.GetOffers";

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
