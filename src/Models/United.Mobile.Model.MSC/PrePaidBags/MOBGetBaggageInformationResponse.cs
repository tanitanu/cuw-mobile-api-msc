using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.PrePaidBags
{
    [Serializable()]
    public class MOBGetBaggageInformationResponse: MOBResponse
    {
        private List<MOBBaggageDetail> baggageDetails; 
        private string checkoutResourceUrl;  
        private List<MOBSection> baggageContentList; // Carry-on baggage and checked baggage

        public List<MOBBaggageDetail> BaggageDetails
        {
            get { return baggageDetails; }
            set { baggageDetails = value; }
        }

        public string CheckoutResourceUrl
        {
            get { return checkoutResourceUrl; }
            set { checkoutResourceUrl = value; }
        }
        public List<MOBSection> BaggageContentList
        {
            get { return baggageContentList; }
            set { baggageContentList = value; }
        }
    }
}
