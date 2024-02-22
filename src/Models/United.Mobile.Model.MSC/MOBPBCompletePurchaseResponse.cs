using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBPBCompletePurchaseResponse: MOBResponse
    {
        private string recordLocator = string.Empty;
        private string lastName = string.Empty;
        private MOBPostBookingAncillaryConfirmationResponse confirmationResponse = null;

        
        public string RecordLocator
        {
            get { return this.recordLocator; }
            set { this.recordLocator = value; }
        }

        public string LastName
        {
            get { return this.lastName; }
            set { this.lastName = value; }
        }

        public MOBPostBookingAncillaryConfirmationResponse ConfirmationResponse
        {
            get { return this.confirmationResponse; }
            set { this.confirmationResponse = value; }
        }
    }
}
