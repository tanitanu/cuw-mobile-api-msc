using System;
using System.Collections.Generic;
using United.Definition.Shopping;
using United.Mobile.Model.Common;
namespace United.Definition.SSR
{
    [Serializable()]
    public class MOBTravelSpecialNeed
    {
        private string value;
        private string displayDescription;
        private string type;
        private string code;
        private string displaySequence;
        private List<MOBTravelSpecialNeed> subOptions;
        private List<MOBItem> messages;
        private string registerServiceDescription;
        private string subOptionHeader;
        private bool isDisabled;
        private string informationLink;
        private MOBDimensions wheelChairDimensionInfo;

        public string InformationLink
        {
            get { return informationLink; }
            set { informationLink = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }
        public string DisplaySequence
        {
            get { return this.displaySequence; }
            set { this.displaySequence = string.IsNullOrWhiteSpace(value) ? null : value; }        
        }

        public string Value
        {
            get { return value; }
            set { this.value = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }

        public string Code
        {
            get { return code; }
            set { code = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }

        public string DisplayDescription
        {
            get { return displayDescription; }
            set { displayDescription = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }

        public string RegisterServiceDescription
        {
            get { return registerServiceDescription; }
            set { registerServiceDescription = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }

        /// <summary>
        /// optional
        /// </summary>
        public string Type
        {
            get { return type; }
            set { type = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }

        public List<MOBItem> Messages
        {
            get { return messages; }
            set { messages = value; }
        }

        public List<MOBTravelSpecialNeed> SubOptions
        {
            get { return subOptions; }
            set { subOptions = value; }
        }

        public string SubOptionHeader
        {
            get { return subOptionHeader; }
            set { subOptionHeader = value; }
        }

        public bool IsDisabled
        {
            get { return isDisabled; }
            set { isDisabled = value; }
        }
        public MOBDimensions WheelChairDimensionInfo
        {
            get { return wheelChairDimensionInfo; }
            set { wheelChairDimensionInfo = value; }
        }
    }
}
