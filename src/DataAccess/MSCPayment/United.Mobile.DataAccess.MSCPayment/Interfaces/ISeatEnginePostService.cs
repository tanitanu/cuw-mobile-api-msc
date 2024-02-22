using System;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface ISeatEnginePostService
    {
        Task<string> SeatEnginePostNew(string transactionId, string url, string contentType, string token, string requestData);
    }
}
