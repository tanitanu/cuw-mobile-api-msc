using System;

namespace United.Mobile.Model.MSC
{
    public class InsertClubPassData
    {
        public string MpAccountNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string ClubPassCode { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
        public string EMail { get; set; } = string.Empty;
        public string BarCodeString { get; set; } = string.Empty;
        public double PaymentAmount { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string DeviceType { get; set; } = string.Empty;
        public bool IsTest { get; set; }
        public string RecordLocator { get; set; } = string.Empty;
    }
}
