using System;

namespace United.Definition.FFC
{
    [Serializable]
    public class TravelCredit
    {
        public long CertIntNum = 0;
        public string PromoID { get; set; }
        public string CertPin { get; set; }
        public string InitialValue = "0.00";
        public string UsedValue = "0.00";
        public string CurrentValue = "0.00";
        public string TotalUsedValue = "0.00";
        public string CertStatCode = "N";
        public string CertExpDate = "01/01/1900";
        public string OrigIssueDate = "01/01/1900";
        public string OrigPlaceOfIssue { get; set; }
        public string IssuanceReason { get; set; }
        public string CertificateNumber { get; set; }
        public string OrigTicketNumber { get; set; }
        public string OrigPNR { get; set; }
        public string OrigOrigin { get; set; }
        public string OrigDestination { get; set; }
        public string OrigFltNumber { get; set; }
        public string OrigFltDate { get; set; }
        public string OrigTicketAmt { get; set; }
        public string EffTravelDate = "01/01/1900";
        public string Message { get; set; }

    }
}
