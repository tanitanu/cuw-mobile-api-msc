using United.Service.Presentation.ReservationResponseModel;
using United.Mobile.Model.Common;
namespace United.Definition.CFOP
{
    public class MOBGetMerchOfferRequest : MOBRequest
    {
        public string ShoppingSessionId { get; set; }
        public string ChannelId { get; set; }
        public string ChannelName { get; set; }
        public ReservationDetail ReservationDetail { get; set; }
        public string Flow { get; set; }
        public string MPNumber { get; set; }
    }
}
