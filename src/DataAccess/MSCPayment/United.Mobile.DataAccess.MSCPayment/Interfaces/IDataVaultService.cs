using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface IDataVaultService
    {
        Task<string> GetPersistentToken(string token, string requestData,string url, string sessionId);
        Task<string> GetCCTokenWithDataVault(string token, string request, string sessionId);
    }
}
