using System.Threading.Tasks;
using United.Definition;
using United.Definition.FormofPayment;
using United.Persist.Definition.Shopping;


namespace United.Mobile.MoneyPlusMiles.Domain
{
    public interface IMoneyPlusMilesBusiness
    {
        Task<MOBFOPResponse> GetMoneyPlusMiles(GetMoneyPlusMilesRequest request);
        Task<MOBFOPResponse> ApplyMoneyPlusMiles(ApplyMoneyPlusMilesOptionRequest request);

        Task GetMoneyPlusMilesOptionsForFinalRTIScreen(MOBRegisterSeatsRequest request, MOBBookingRegisterSeatsResponse response, United.Persist.Definition.Shopping.Session session);

        Task<MOBFOPResponse> GetMilesPlusMoneyOptions(Session session, GetMoneyPlusMilesRequest request);

    }
}
