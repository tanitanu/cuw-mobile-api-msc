using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface IPassDetailService
    {
        public Task<string> GetLoyaltyUnitedClubIssuePass(string token, string action, string request, string sessionId);
    }
}
