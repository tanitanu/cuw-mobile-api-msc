using System.Threading.Tasks;
using United.Definition;
using United.Definition.FormofPayment;

namespace United.Mobile.TravelCredit.Domain
{
    public interface ITravelCreditBusiness
    {
        Task<MOBFutureFlightCreditResponse> FutureFlightCredit(MOBFutureFlightCreditRequest request);
        Task<MOBFutureFlightCreditResponse> RemoveFutureFlightCredit(MOBFOPFutureFlightCreditRequest request);
        Task<MOBFOPResponse> LookUpTravelCredit(MOBFOPLookUpTravelCreditRequest request);
        Task<MOBFOPResponse> ManageTravelCredit(MOBFOPManageTravelCreditRequest request);
        Task<MOBFutureFlightCreditResponse> FindFutureFlightCreditByEmail(MOBFutureFlightCreditRequest mOBFutureFlightCredit);
    }
}