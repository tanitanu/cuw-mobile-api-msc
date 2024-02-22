using System.Threading.Tasks;
using United.Persist.Definition.Shopping;

namespace United.Common.Helper
{
    public interface IMSCShoppingSessionHelper
    {
        Task<Session> GetBookingFlowSession(string sessionId, bool isBookingFlow = true);
        Task<Session> GetValidateSession(string sessionId, bool isBookingFlow, bool isViewRes_CFOPFlow);
        Task<Session> CreateShoppingSession(int applicationId, string deviceId, string appVersion, string transactionId, string mileagPlusNumber, string employeeId, bool isBEFareDisplayAtFSR = false, bool isReshop = false, bool isAward = false, string shoppingSessionId = "");
    }
}
