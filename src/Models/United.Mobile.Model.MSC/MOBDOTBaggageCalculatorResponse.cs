using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBDOTBaggageCalculatorResponse : MOBResponse
    {
        public MOBDOTBaggageCalculatorResponse()
        {

        }
        public MOBDOTBaggageCalculatorResponse(bool getDOTStaticInfoText)
        {
            if (getDOTStaticInfoText)
            {
                faqsCheckedBagsTitle = System.Configuration.ConfigurationManager.AppSettings["DOTBaggageFAQsTitle"].Split('|')[0].ToString();
                myCheckedBagServiceChargesDesc = System.Configuration.ConfigurationManager.AppSettings["MyCheckedBagServiceChargesDesc"].Split('|')[0].ToString();
            }
        }
        private MOBDOTBaggageInfoRequest request;
        private List<MOBDOTBaggageAdditionalDetails> checkedAndOtherBagFees;
        private string faqsCheckedBagsTitle = string.Empty;
        private List<MOBDOTBaggageFAQ> baggageFAQs;
        private string myCheckedBagServiceChargesDesc = string.Empty;
        private bool isAnyFlightSearch;

        public MOBDOTBaggageInfoRequest Request
        {
            get
            {
                return this.request;
            }
            set
            {
                this.request = value;
            }
        }
        public List<MOBDOTBaggageAdditionalDetails> CheckedAndOtherBagFees
        {
            get
            {
                return this.checkedAndOtherBagFees;
            }
            set
            {
                this.checkedAndOtherBagFees = value;
            }
        }
        public string FAQsCheckedBagsTitle
        {
            get { return this.faqsCheckedBagsTitle; }
            set { this.faqsCheckedBagsTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public List<MOBDOTBaggageFAQ> BaggageFAQs
        {
            get { return this.baggageFAQs; }
            set { this.baggageFAQs = value; }
        }
        public string MyCheckedBagServiceChargesDesc
        {
            get { return this.myCheckedBagServiceChargesDesc; }
            set { this.myCheckedBagServiceChargesDesc = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public bool IsAnyFlightSearch
        {
            get { return isAnyFlightSearch; }
            set { isAnyFlightSearch = value; }
        }
    }
}
