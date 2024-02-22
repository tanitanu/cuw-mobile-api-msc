using System.Threading.Tasks;
using United.Definition.FormofPayment;

namespace United.Mobile.InFlightPurchase.Domain
{
    public interface IInFlightPurchaseBusiness
    {
        Task<MOBInFlightPurchaseResponse> EiligibilityCheckToAddNewCCForInFlightPurchase(MOBInFlightPurchaseRequest request);
        Task<MOBInFlightPurchaseResponse> SaveCCPNROnlyForInflightPurchase(MOBSavedCCInflightPurchaseRequest request);
        Task<MOBInFlightPurchaseResponse> VerifySavedCCForInflightPurchase(MOBSavedCCInflightPurchaseRequest request);
    }
}
