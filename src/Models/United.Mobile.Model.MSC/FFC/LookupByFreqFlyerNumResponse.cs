
using System;
using System.Collections.Generic;
using United.Service.Presentation.CommonModel;

namespace United.Definition.FFC
{
    public class LookupByFreqFlyerNumResponse
    {
        public ETCCertificates ETCCertificates { get; set; }
        public FFCRCertificates FFCRCertificates { get; set; }
        public FFCCertificates FFCCertificates { get; set; }
        public ServiceResponse Response { get; set; }        
        public string Message { get; set; }       
        public bool IsSuccessful { get; set; }
    }

    public class ETCCertificates
    {
        public List<ETCCertificate> CertificateList { get; set; }
        public string Message { get; set; }
        public bool IsSuccessful { get; set; }
    }   
    public class ETCCertificate
    {
        public long CertIntNum = 0;
        public string PromoID = string.Empty;
        public string CertPin = string.Empty;
        public string InitialValue = "0.00";
        public string UsedValue = "0.00";
        public string CurrentValue = "0.00";
        public string TotalUsedValue = "0.00";
        public string CertStatCode = "N";
        public string FirstName = string.Empty;
        public string LastName = string.Empty;
        public string CertExpDate = "01/01/1900";
        public string OrigIssueDate = "01/01/1900";
        public string OrigPlaceOfIssue = string.Empty;
        public string IssuanceReason = string.Empty;
        public string CertificateNumber = string.Empty;
        public string ProductCode = string.Empty;
        public string ProductId = string.Empty;
        public string PNR = string.Empty;
    }
    public class FFCRCertificates
    {
        public List<FFCRCertificate> CertificateList { get; set; }
        public string Message { get; set; }
        public bool IsSuccessful { get; set; }
    }
    public class FFCRCertificate
    {
        public short PassengerIndex { get; set; }
        public bool HasActiveTCMatch { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string ZipExt { get; set; }
        public string Phone { get; set; }
        public List<TravelCredit> TravelCreditList { get; set; }
        public string DateOfBirth = "01/01/1900";
    }   

    public class FFCCertificates
    {
        public List<FFCCertificate> CertificateList { get; set; }
        public string Message { get; set; }
        public bool IsSuccessful { get; set; }
    }
    public class FFCCertificate
    {
        public List<Segment> Segment { get; set; }
        public List<Traveller> Traveller { get; set; }
        public bool HasFFC { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsFarelockPNR { get; set; }
        public string Text { get; set; }
        public string RecLoc { get; set; }
        public string RecLocCrtDt { get; set; }
        public string FFCExpirationDt { get; set; }
    }
    public class Segment
    {
        public string OpCarrCd { get; set; }
        public int FltNbr { get; set; }
        public string Orig { get; set; }
        public string Dest { get; set; }
        public string DepDtmLcl { get; set; }
        public string ArrDtmLcl { get; set; }
        public string BkngCls { get; set; }
        public string SegActnCd { get; set; }
    }

    public class Traveller
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
   
}