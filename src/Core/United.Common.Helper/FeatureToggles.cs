using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Utility.Helper;

namespace United.Common.Helper
{
    public class FeatureToggles : IFeatureToggles
    {
        private readonly IFeatureSettings _featureSettings;
        private readonly IConfiguration _configuration;
        public FeatureToggles(IConfiguration configuration, IFeatureSettings featureSettings)
        {
            _featureSettings = featureSettings;
            _configuration = configuration;
        }
        public async Task<bool> IsEnableConfirmationCTOTile(int applicationId, string appVersion)
        {
            return (await _featureSettings.GetFeatureSettingValue("EnableConfirmationCTOTile").ConfigureAwait(false) &&
                GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Andriod_EnableConfirmationCTOTile_AppVersion"), _configuration.GetValue<string>("Iphone_EnableConfirmationCTOTile_AppVersion")));
        }

        public async Task<bool> IsEnabledTripInsuranceV2(int applicationId, string appVersion, List<MOBItem> catalogItems = null)
        {
            return (await _featureSettings.GetFeatureSettingValue("EnableTripInsuranceV2").ConfigureAwait(false) && (catalogItems != null && catalogItems.Count > 0 &&
            catalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.EnableTripInsuranceV2).ToString() || a.Id == ((int)AndroidCatalogEnum.EnableTripInsuranceV2).ToString())?.CurrentValue == "1")
                && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableTripInsuranceV2_AppVersion"), _configuration.GetValue<string>("IPhone_EnableTripInsuranceV2_AppVersion")));
        }

        public async Task<bool> IsEnabledExpressCheckoutFlow(int applicationId, string appVersion, List<MOBItem> catalogItems = null)
        {
            return (await _featureSettings.GetFeatureSettingValue("EnableExpressCheckoutChanges").ConfigureAwait(false) && (catalogItems != null && catalogItems.Count > 0 &&
            catalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.EnableExpressCheckout).ToString() || a.Id == ((int)AndroidCatalogEnum.EnableExpressCheckout).ToString())?.CurrentValue == "1")
                && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableExpressCheckout_AppVersion"), _configuration.GetValue<string>("iPhone_EnableExpressCheckout_AppVersion")));
        }

        public async Task<bool> IsEnableAutoRefundMessageOnViewResCheckOut(int applicationId, string appVersion)
        {
            return (await _featureSettings.GetFeatureSettingValue("IsEnableAutoRefundMessageOnViewResCheckOut").ConfigureAwait(false) &&
                GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_IsEnableAutoRefundMessageOnViewResCheckOut_AppVersion"), _configuration.GetValue<string>("IPhone_IsEnableAutoRefundMessageOnViewResCheckOut_AppVersion")));
        }

        public async Task<bool> IsEnableMFOPCheckinBags(int applicationId, string appVersion)
        {
            return ConfigUtility.IsEnableMFOPCheckinBags(applicationId, appVersion) && await _featureSettings.GetFeatureSettingValue("EnableMfopForCheckinBags");
        }

        public async Task<bool> IsEnableETCCreditsInBookingFeature(List<MOBItem> catalogItems = null)
        {
            return (await _featureSettings.GetFeatureSettingValue("EnableFSRETCTravelCreditsFeature").ConfigureAwait(false) && (catalogItems != null && catalogItems.Count > 0 &&
                catalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.EnableETCCreditsInBooking).ToString() || a.Id == ((int)AndroidCatalogEnum.EnableETCCreditsInBooking).ToString())?.CurrentValue == "1")
            );
        }
        public async Task<bool> IsEnableFreeSeatConfirmationMessageOnMyTrips(int appid, string appVersion)
        {
            return (await _featureSettings.GetFeatureSettingValue("IsEnableFreeSeatConfirmationMessageOnMyTrips").ConfigureAwait(false) && GeneralHelper.IsApplicationVersionGreaterorEqual(appid, appVersion, _configuration.GetValue<string>("IsEnableUpselltoUpsellInManageRes_AppVersion_Android"), _configuration.GetValue<string>("IsEnableUpselltoUpsellInManageRes_AppVersion_Iphone")));

        }
    }
}
