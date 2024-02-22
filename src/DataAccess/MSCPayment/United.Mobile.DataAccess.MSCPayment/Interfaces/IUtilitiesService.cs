using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface IUtilitiesService
    {
        Task<string> ValidateMPNames(string token, string requestData, string sessionId, string path = "");
        Task<T> ValidateMileagePlusNames<T>(string token, string requestData, string sessionId, string path);
        Task<string> ValidatePhoneWithCountryCode(string token, string urlPath, string sessionId);

    }
}
