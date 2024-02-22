using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface IPurchaseMerchandizingService
    {
        Task<string> GetInflightPurchaseEligibility(string token, string request, string sessionId);
    }
}
