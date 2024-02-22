using United.Mobile.Model.Common;
using United.Service.Presentation.PersonalizationResponseModel;
using United.Service.Presentation.ProductResponseModel;

namespace United.Persist.Definition.Merchandizing
{
    public class GetCceBagOffers : ProductOffer, IPersist
    {
        public GetCceBagOffers() { }

        #region IPersist Members
        private string objectName = "United.Persist.Definition.Merchandizing.GetCceBagOffers";

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
