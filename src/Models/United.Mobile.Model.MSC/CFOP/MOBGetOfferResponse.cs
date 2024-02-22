using United.Service.Presentation.ProductResponseModel;
using United.Mobile.Model.Common;
namespace United.Definition.CFOP
{
    public class MOBGetOfferResponse : MOBResponse
    {
        public string ShoppingSessionId { get; set; }
        public ProductOffer ProductOffer { get; set; }
    }
}
