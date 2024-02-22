using System;

namespace United.Definition.CancelReservation
{
    [Serializable]
    public class MOBCancelRefundInfoRequest : MOBModifyReservationRequest
    {
        private string pnr = string.Empty;
        private string lastName = string.Empty;
        private bool isVersionAllowAwardCancel = false;
        public string Token { set; get; }

        public bool IsAward { set; get; }
        public bool isPNRELF { set; get; }
        public string PNR
        {
            get { return pnr; }
            set { pnr = value; }
        }

        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public bool IsVersionAllowAwardCancel
        {
            get { return isVersionAllowAwardCancel; }
            set { isVersionAllowAwardCancel = value; }
        }
    }
}
