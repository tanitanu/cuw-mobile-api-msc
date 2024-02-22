using System;
using System.Runtime.Serialization;

namespace United.Definition.CancelReservation
{
    [Serializable]
    public class CancelReservationResponse
    {
        public StatusType Status;
        public string Message;
        public string RecordLocator;
        public string Error;
    }

    [Serializable]
    public enum StatusType
    {
        [EnumMember]
        Failure,
        [EnumMember]
        Success,
    }
}
