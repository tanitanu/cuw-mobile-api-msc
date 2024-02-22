using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Services
{
    public interface IGetPNRByRecordLocatorService
    {
        Task<string> GetPNRByRecordLocator(string request, string transactionId, string path);
    }
}