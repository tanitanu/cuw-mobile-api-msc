using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Common.Helper
{
    public interface IFeatureToggles
    {
        Task<bool> IsEnableConfirmationCTOTile(int applicationId, string appVersion);
        Task<bool> IsEnabledTripInsuranceV2(int applicationId, string appVersion, List<MOBItem> catalogItems = null);
        Task<bool> IsEnabledExpressCheckoutFlow(int applicationId, string appVersion, List<MOBItem> catalogItems = null);
        Task<bool> IsEnableAutoRefundMessageOnViewResCheckOut(int applicationId, string appVersion);
        Task<bool> IsEnableMFOPCheckinBags(int applicationId, string appVersion);
        Task<bool> IsEnableETCCreditsInBookingFeature(List<MOBItem> catalogItems = null);
        Task<bool> IsEnableFreeSeatConfirmationMessageOnMyTrips(int appid, string appVersion);
    }
}
