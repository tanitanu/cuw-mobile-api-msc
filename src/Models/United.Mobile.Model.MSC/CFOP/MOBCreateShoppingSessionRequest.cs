using United.Service.Presentation.ReservationModel;
using United.Mobile.Model.Common;
namespace United.Definition.CFOP
{
    public class MOBCreateShoppingSessionRequest : MOBRequest
    {
        public Reservation Reservation { get; set; }
        public string MPNumber { get; set; }
        public string ShoppingSessionId { get; set; }
    }
}
