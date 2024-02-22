using System.Threading.Tasks;
using United.Definition;
using United.Definition.FormofPayment;
using United.Mobile.Model.MSC.FormofPayment;

namespace United.Mobile.ETC.Business
{
    public interface IETCBusiness
    {
        Task<MOBFOPTravelerCertificateResponse> TravelerCertificate(MOBFOPTravelerCertificateRequest request);
        Task<MOBFOPTravelerCertificateResponse> PersistFOPBillingContactInfo_ETC(MOBFOPBillingContactInfoRequest request);
        Task<MOBFOPTravelerCertificateResponse> ApplyMileagePricing(MOBRTIMileagePricingRequest request);
    }
}
