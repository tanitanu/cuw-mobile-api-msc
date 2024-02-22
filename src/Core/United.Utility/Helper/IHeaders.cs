using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using United.Mobile.Model;
using United.Mobile.Model.Common;

namespace United.Common.Helper
{
    public interface IHeaders
    {       
        Task<bool> SetHttpHeader(string deviceId, string applicationId, string appVersion, string transactionId, string languageCode, string sessionId);
        HttpContextValues ContextValues { get; set; }        
    }
}
