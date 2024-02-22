using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Mobile.Model.DynamoDb.Common;


namespace United.Mobile.DataAccess.Common
{
    public interface IAuroraMySqlService
    {
        Task<bool> InsertpaymentRecord(PaymentDB paymentRequest);
        Task<List<MOBFeatureSetting>> GetFeatureSettingsByAPIName(string apiName);
        Task<MOBFeatureSetting> InsertFeatureSettings(MOBFeatureSetting featureSetting);
        Task<List<MOBContainerIPAddressDetails>> GetContainerIPAddressesByService(string apiName);
        Task<bool> UpdateIsManuallyRefreshedToggle(string serviceName, string containerIpAdressList);
        Task<bool> InsertContainerIPAddress(MOBContainerIPAddressDetails request);
        Task<bool> DeleteContainerIPAddress(MOBContainerIPAddressDetails request);
        Task<bool> IsAllContainersRefreshed(string serviceName);
    }
}
