using System;
using United.Service.Presentation.CommonModel;

namespace United.Definition.FFC
{
    [Serializable()]
    public class LookupByEmailResponse
    {
        //public List<FFCRCertificate> CertificateList { get; set; }
        public TravelCreditDetail TravelCreditDetail { get; set; }
        public ServiceResponse Response { get; set; }
        public string Message { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
