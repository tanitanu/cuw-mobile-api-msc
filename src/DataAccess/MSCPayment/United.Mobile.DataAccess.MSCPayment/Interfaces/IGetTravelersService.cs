using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface IGetTravelersService
    {
        Task<string> GetTravelersSavedCCDetails(string path, string request, string sessionId, string token);
        Task<string> LookupAndSaveProfileCard(string path, string request, string sessionId, string token);
        Task<string> GetCslApiResponse(string path, string request, string sessionId, string token);
        Task<string> OptOutMPCardInflightPurchase(string path, string request, string sessionId, string token);
        Task<string> OptOutBookingCardInflightPurchase(string path, string request, string sessionId, string token);
    }
}
