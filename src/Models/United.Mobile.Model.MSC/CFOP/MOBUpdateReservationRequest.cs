using United.Mobile.Model.Common;
namespace United.Definition.CFOP
{
    public class MOBUpdateReservationRequest : MOBRequest
    {
        public Service.Presentation.ReservationModel.Reservation Reservation { get; set; }
        public string ShoppingSessionId { get; set; }
    }
}
