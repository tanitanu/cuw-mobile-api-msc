using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface IGetMPNumberService
    {
        Task<string> GetMPNumberByEmployeeId(string path, string sessionId, string token);
        Task<string> GetSavedCCForMileaguePlusMember(string path,string sessionId, string token);
        Task<string> GetProfileAddressByKey(string path, string sessionId, string token);
    }
}
