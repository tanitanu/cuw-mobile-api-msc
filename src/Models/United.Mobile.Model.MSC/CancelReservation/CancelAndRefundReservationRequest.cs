namespace United.Definition.CancelReservation
{
    public class CancelAndRefundReservationRequest
    {
        public decimal AwardRedepositFeeTotal { get; set; }
        public decimal AwardRedepositFee { get; set; }
        public string RecordLocator { get; set; }
        public string EmailAddress { get; set; }
        public string LastName { get; set; }
        public int Channel { get; set; }
        public string PointOfSale { get; set; }
        public string MileagePlusNumber { get; set; }
        public string QuoteType { get; set; }
        public decimal RefundAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string RefundMiles { get; set; }
        public United.Service.Presentation.PaymentModel.FormOfPayment FormOfPayment { get; set; }
    }
}
