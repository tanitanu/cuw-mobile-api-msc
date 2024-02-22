using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Persist.Definition.Shopping;

namespace United.Mobile.DataAccess.Common
{
    public interface IDPService
    {
        public Task<string> GetAnonymousToken(int applicationId, string deviceId, IConfiguration configuration);
        public Task<string> GetAnonymousToken(int applicationId, string deviceId, IConfiguration configuration, string configSectionKey);

        Task<string> GetAnonymousTokenV2(int applicationId, string deviceId, IConfiguration configuration, string configSectionKey, bool saveToCache = false);
        Task<string> GetAndSaveAnonymousToken(int applicationId, string deviceId, IConfiguration configuration, string configSectionKey, Session session);
    }
}