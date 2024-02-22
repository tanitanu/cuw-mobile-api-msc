using System.Collections.Generic;
using United.Mobile.Model.Common;

namespace United.Common.Helper.MSCPayment.Interfaces
{
    public interface IMSCPkDispenserPublicKey
    {
        System.Threading.Tasks.Task<string> GetCachedOrNewpkDispenserPublicKey(int appId, string appVersion, string deviceId, string transactionId, string token, List<MOBItem> catalogItems = null, string flow = "");
        System.Threading.Tasks.Task<string> GetCachedOrNewpkDispenserPublicKey(int appId, string appVersion, string deviceId, string transactionId, string token, string flow, List<MOBItem> catalogItems = null);
        string GetNewPublicKeyPersistSessionStaticGUID(int applicationId);
    }
}
