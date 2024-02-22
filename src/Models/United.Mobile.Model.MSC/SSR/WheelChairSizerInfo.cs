using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using United.Definition.Shopping;
using United.Mobile.Model.Common;

namespace United.Definition.SSR
{
    [Serializable()]
    public class WheelChairSizerInfo
    {
        private MOBAlertMessages wheelChairErrorMessages;
        private string imageUrl1;
        private string imageUrl2;
        private string wcHeaderMsg;
        private string wcBodyMsg;
        private bool isWheelChairSizerEligible = true;
        private MOBDimensions dimensionInfo;
        private bool isSelected;
        private string disclaimer;
        private string redirectUrl;
        private List<MOBItem> batteryTypes;
        private string selectedBatteryType;
        public MOBAlertMessages WheelChairErrorMessages
        {
            get { return wheelChairErrorMessages; }
            set { wheelChairErrorMessages = value; }
        }
        public string ImageUrl1
        {
            get { return imageUrl1; }
            set { imageUrl1 = value; }
        }
        public string ImageUrl2
        {
            get { return imageUrl2; }
            set { imageUrl2 = value; }
        }
        public string WcHeaderMsg
        {
            get { return wcHeaderMsg; }
            set { wcHeaderMsg = value; }
        }
        public string WcBodyMsg
        {
            get { return wcBodyMsg; }
            set { wcBodyMsg = value; }
        }
        public bool IsWheelChairSizerEligible
        {
            get { return isWheelChairSizerEligible; }
            set { isWheelChairSizerEligible = value; }
        }
        public MOBDimensions DimensionInfo
        {
            get { return dimensionInfo; }
            set { dimensionInfo = value; }
        }
        public string Disclaimer
        {
            get { return disclaimer; }
            set { disclaimer = value; }
        }
        public string RedirectUrl
        {
            get { return redirectUrl; }
            set { redirectUrl = value; }
        }
        public List<MOBItem> BatteryTypes
        {
            get { return batteryTypes; }
            set { batteryTypes = value; }
        }
        public string SelectedBatteryType
        {
            get { return selectedBatteryType; }
            set { selectedBatteryType = value; }
        }
    }
}
