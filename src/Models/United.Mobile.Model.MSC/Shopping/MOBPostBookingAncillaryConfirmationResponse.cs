using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBPostBookingAncillaryConfirmationResponse: MOBResponse
    {
        private string emailId = string.Empty;
        private List<MOBPBSegment> segments;
        private string totalPrice; // $54
        private string paymentInformation;  // $54(**2929) 

        public string EmailId
        {
            get
            {
                return this.emailId;
            }
            set
            {
                this.emailId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToLower();
            }
        }
        public List<MOBPBSegment> Segments
        {
            get
            {
                return this.segments;
            }
            set
            {
                this.segments = value;
            }
        }
        public string TotalPrice
        {
            get
            {
                return this.totalPrice;
            }
            set
            {
                this.totalPrice = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public string PaymentInformation
        {
            get
            {
                return this.paymentInformation;
            }
            set
            {
                this.paymentInformation = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
