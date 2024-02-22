using System.Collections.Generic;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.Product.Interfaces
{
    public interface IShoppingCartService
    {
        Task<string> CreateCart(string token, string request, string sessionId);
        Task<T> GetCartInformation<T>(string token, string action, string request, string sessionId);
        Task<T> RegisterOrRemoveCoupon<T>(string token, string action, string request, string sessionId);
        Task<T> RegisterFlights<T>(string token, string action, string request, string sessionId);
        Task<T> RegisterOffers<T>(string token, string action, string request, string sessionId);
        Task<string> RegisterFareLockReservation(string token, string action, string request, string sessionId);
        Task<string> RegisterCheckinSeats(string token, string action, string request, string sessionId);
        Task<string> RegisterSameDayChange(string token, string action, string request, string sessionId);
        Task<string> RegisterFormsOfPayments_CFOP(string token, string action, string request, string sessionId, Dictionary<string, string> additionalHeaders, string clonedRequest = null);
        Task<string> ClearSeats(string token, string action, string request, string sessionId);
        Task<string> RegisterSeats_CFOP(string token, string action, string request, string sessionId);
        Task<string> MetaSyncUserSession<T>(string token, string sessionId, string action, string request);
        Task<string> ShoppingCartServiceCall(string token, string action, string request, string sessionId);
        Task<string> DeletePayment(string token, string action, string sessionId);
        Task<T> GetRegisterSeats<T>(string token, string action, string sessionId, string jsonRequest);
        Task<T> GetAsync<T>(string actionName, string token, string sessionId);


    }
}
