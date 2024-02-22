using System;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBPriorityBoardingSelectionRequest : MOBRequest
    {
        private string sessionId = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private string selectedCustomerInSegments = string.Empty; // 1,2

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }
        public string MileagePlusNumber
        {
            get { return mileagePlusNumber; }
            set { mileagePlusNumber = value; }
        }
        public string SelectedCustomerInSegments
        {
            get { return selectedCustomerInSegments; }
            set { selectedCustomerInSegments = value; }
        }
    }
}
