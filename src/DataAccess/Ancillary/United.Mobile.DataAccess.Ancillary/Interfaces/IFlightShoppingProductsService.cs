using System.Threading.Tasks;

namespace United.Mobile.DataAccess.Product.Interfaces
{
    public interface IFlightShoppingProductsService
    {
        Task<string> GetProducts(string token, string sessionId, string request, string transationId);
        Task<string> ApplyCSLMilesPlusMoneyOptions(string token, string action, string request, string sessionId);
        Task<string> GetCSLMilesPlusMoneyOptions(string token, string action, string request, string sessionId);

        Task<string> GetTripInsuranceInfo(string token, string action, string request, string sessionId);
    }
}
