using System;
using United.Mobile.Model.Common;
namespace United.Definition.CancelReservation
{
    [Serializable]
    public class MOBRefundResponse : MOBResponse
    {
        public string RecordLocator { get; set; }
        public double RefundAmount { get; set; }
        public bool Success { get; set; }
    }
}