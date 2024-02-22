using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Services
{
    public interface ILoyaltyUCBService
    {
        Task<string> GetLoyaltyBalance(string token, string mpnumber, string sessionId);
    }
}
