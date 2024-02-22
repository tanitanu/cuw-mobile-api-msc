using System.Threading.Tasks;
using United.Definition;

namespace United.Mobile.Services.RegisterOffers.Domain
{
    public interface IMSCRegisterBusiness
    {
        Task<MOBRegisterOfferResponse> RegisterOffers(MOBRegisterOfferRequest request);
        Task<MOBRegisterOfferResponse> RegisterUsingMiles(MOBRegisterUsingMilesRequest request);
        Task<MOBRegisterOfferResponse> RegisterBags(MOBRegisterBagsRequest request);
        Task<MOBRegisterOfferResponse> RegisterBagDetails(MOBRegisterBagDetailsRequest request);
        Task<MOBRegisterOfferResponse> RegisterSameDayChange(MOBRegisterSameDayChangeRequest request);
        Task<MOBRegisterOfferResponse> RegisterCheckinSeats(MOBRegisterCheckinSeatsRequest request);
        Task<MOBRegisterSeatsResponse> RegisterSeats(MOBRegisterSeatsRequest request);
    }
}
