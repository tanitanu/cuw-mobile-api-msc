using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Gamification
{
    [Serializable]
    public class MOBPersonalThresholdPromotionResponse : MOBResponse
    {

        private MOBPersonalThresholdWidgetUnregistered widget_Unregistered;
        private MOBPersonalThresholdWidget widget;
        private string state;
        private string headline;
        private string link;
        private string registered_Headline;
        private string qualification_Headline;
        private string activity_Notice;
        private string cta;
        private MOBPersonalThresholdTermsandConditions terms_and_Conditions;
        private List<MOBPersonalThresholdStep> steps;
        private string total_Footer;
        private string mileagePlusNumber = string.Empty;
        private string custID = string.Empty;
        private string promoCode = string.Empty;
        private string memberPromotionID = string.Empty;
        private string altRefID1 = string.Empty;
        private bool? isRegistered = false;
        private DateTime expirationDate;

        public MOBPersonalThresholdWidgetUnregistered Widget_Unregistered
        {
            get { return widget_Unregistered; }
            set { widget_Unregistered = value; }
        }
        public MOBPersonalThresholdWidget Widget
        {
            get { return widget; }
            set { widget = value; }
        }
        public string State
        {
            get { return state; }
            set { state = value; }
        }
        public string Headline
        {
            get { return headline; }
            set { headline = value; }
        }
        public string Link
        {
            get { return link; }
            set { link = value; }
        }
        public string Registered_Headline
        {
            get { return registered_Headline; }
            set { registered_Headline = value; }
        }
        public string Qualification_Headline
        {
            get { return qualification_Headline; }
            set { qualification_Headline = value; }
        }
        public string Activity_Notice
        {
            get { return activity_Notice; }
            set { activity_Notice = value; }
        }
        public string CTA
        {
            get { return cta; }
            set { cta = value; }
        }

        public MOBPersonalThresholdTermsandConditions Terms_and_Conditions
        {
            get { return terms_and_Conditions; }
            set { terms_and_Conditions = value; }
        }
        public List<MOBPersonalThresholdStep> Steps
        {
            get { return steps; }
            set { steps = value; }
        }

        public string Total_Footer
        {
            get { return total_Footer; }
            set { this.total_Footer = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string MileagePlusNumber
        {
            get { return mileagePlusNumber; }
            set { this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper(); }
        }
        public string CustID
        {
            get
            {
                return this.custID;
            }
            set
            {
                this.custID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public string PromoCode
        {
            get { return promoCode; }
            set { promoCode = value; }
        }
        public string MemberPromotionID
        {
            get
            {
                return this.memberPromotionID;
            }
            set
            {
                this.memberPromotionID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public string AltRefID1
        {
            get
            {
                return this.altRefID1;
            }
            set
            {
                this.altRefID1 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public bool? IsRegistered
        {
            get
            {
                return this.isRegistered;
            }
            set
            {
                this.isRegistered = value;
            }
        }
        public DateTime ExpirationDate
        {
            get
            {
                return this.expirationDate;
            }
            set
            {
                this.expirationDate = value;
            }
        }

    }
    [Serializable]
    public class MOBPersonalThresholdWidgetUnregistered
    {
        private string tagline;
        private string tagline_MD;
        private string tagline_SM;
        private string cta;
        private string text;
        private string text_MD;
        private string legal;
        private string legal_MD;
        private object icons;

        public string Tagline
        {
            get { return tagline; }
            set { tagline = value; }
        }
        public string Tagline_MD
        {
            get { return tagline_MD; }
            set { tagline_MD = value; }
        }
        public string Tagline_SM
        {
            get { return tagline_SM; }
            set { tagline_SM = value; }
        }
        public string CTA
        {
            get { return cta; }
            set { cta = value; }
        }
        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        public string Text_MD
        {
            get { return text_MD; }
            set { text_MD = value; }
        }
        public string Legal
        {
            get { return legal; }
            set { legal = value; }
        }
        public string Legal_MD
        {
            get { return legal_MD; }
            set { legal_MD = value; }
        }
        public object Icons
        {
            get { return icons; }
            set { icons = value; }
        }
    }
    [Serializable]
    public class MOBPersonalThresholdIcon
    {
        private string icon;
        private string label;

        public string Icon
        {
            get { return icon; }
            set { icon = value; }
        }
        public string Label
        {
            get { return label; }
            set { label = value; }
        }
    }
    [Serializable]
    public class MOBPersonalThresholdWidget
    {
        private string tagline;
        private string tagline_MD;
        private string tagline_SM;
        private string cta;
        private string text;
        private List<MOBPersonalThresholdIcon> icons;

        public string Tagline
        {
            get { return tagline; }
            set { tagline = value; }
        }
        public string Tagline_MD
        {
            get { return tagline_MD; }
            set { tagline_MD = value; }
        }
        public string Tagline_SM
        {
            get { return tagline_SM; }
            set { tagline_SM = value; }
        }
        public string CTA
        {
            get { return cta; }
            set { cta = value; }
        }
        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        public List<MOBPersonalThresholdIcon> Icons
        {
            get { return icons; }
            set { icons = value; }
        }
    }
    [Serializable]
    public class MOBPersonalThresholdTermsandConditions
    {
        private string label;
        private string is_Active;
        private List<string> body;

        public string Label
        {
            get { return label; }
            set { label = value; }
        }
        public string Is_Active
        {
            get { return is_Active; }
            set { is_Active = value; }
        }
        public List<string> Body
        {
            get { return body; }
            set { body = value; }
        }
        
    }
    [Serializable]
    public class MOBPersonalThresholdStep
    {
        private bool is_Active;
        private string icon;
        private string label;
        private string descr;

        public bool Is_Active
        {
            get { return is_Active; }
            set { is_Active = value; }
        }
        public string Icon
        {
            get { return icon; }
            set { icon = value; }
        }
        public string Label
        {
            get { return label; }
            set { label = value; }
        }
        public string Descr
        {
            get { return descr; }
            set { descr = value; }
        }
    }
    
}
