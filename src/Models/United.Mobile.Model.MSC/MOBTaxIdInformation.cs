using System;
using System.Collections.Generic;
using United.Definition.MPPINPWD;
using United.Definition.Shopping;
using United.Mobile.Model.Common;

namespace United.Definition
{
    [Serializable()]
    public class MOBTaxIdInformation
    {
        private MOBMobileCMSContentMessages taxIdContentMsgs;
        public MOBMobileCMSContentMessages TaxIdContentMsgs
        {
            get { return taxIdContentMsgs; }
            set { taxIdContentMsgs = value; }
        }
        private string selectedTaxIdType;
        public string SelectedTaxIdType
        {
            get { return selectedTaxIdType; }
            set { selectedTaxIdType = value; }
        }
        private string selectedTaxIdValue;
        public string SelectedTaxIdValue
        {
            get { return selectedTaxIdValue; }
            set { selectedTaxIdValue = value; }
        }
        private List<MOBItem> supportedTaxIdTypes;
        public List<MOBItem> SupportedTaxIdTypes
        {
            get { return supportedTaxIdTypes; }
            set { supportedTaxIdTypes = value; }
        }
        private List<MOBComponent> components;
        public List<MOBComponent> Components
        {
            get { return components; }
            set { components = value; }
        }
        private string isTravelerOrPurchaserCountry;
        public string IsTravelerOrPurchaserCountry
        {
            get { return isTravelerOrPurchaserCountry; }
            set { isTravelerOrPurchaserCountry = value; }
        }
        private MOBMobileCMSContentMessages toolTipContentMsgs;
        public MOBMobileCMSContentMessages ToolTipContentMsgs
        {
            get { return toolTipContentMsgs; }
            set { toolTipContentMsgs = value; }
        }
        private bool includeInfantOnLap;
        public bool IncludeInfantOnLap
        {
            get { return includeInfantOnLap; }
            set { includeInfantOnLap = value; }
        }
        private List<List<MOBItem>> selectedValues;
        public List<List<MOBItem>> SelectedValues 
        {
            get { return selectedValues; }
            set { selectedValues = value; }
        }

    }
    [Serializable()]
    public class MOBComponent
    {
        private string title;
        private string placeholderText;
        private string inlineErrorText;
        private bool isOptional;
        private int charRestrictionLimit;
        private List<MOBItem> supportedTypes;
        private string selectedValue;
        private ComponentType componentType;
        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        public string PlaceholderText
        {
            get { return placeholderText; }
            set { placeholderText = value; }
        }
        public string InlineErrorText
        {
            get { return inlineErrorText; }
            set { inlineErrorText = value; }
        }
        public bool IsOptional
        {
            get { return isOptional; }
            set { isOptional = value; }
        }
        public int CharRestrictionLimit
        {
            get { return charRestrictionLimit; }
            set { charRestrictionLimit = value; }
        }
        public List<MOBItem> SupportedTypes
        {
            get { return supportedTypes; }
            set { supportedTypes = value; }
        }
        public string SelectedValue
        {
            get { return selectedValue; }
            set { selectedValue = value; }
        }
        public ComponentType ComponentType
        {
            get { return componentType; }
            set { componentType = value; }
        }
    }

}
