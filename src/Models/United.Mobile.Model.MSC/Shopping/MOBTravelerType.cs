using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBTravelerType
    {
        private int count;
        private string travelerType;
       public int Count {
            get { return count; }
            set { count = value; }
        }
       public string TravelerType {
            get
            {
                return travelerType;
            }
            set
            {
                travelerType = value;
            }
        }
    }

    [Serializable()]
    public class MOBDisplayTravelType
    {
        private int paxID;
        private string paxType;
        private string travelerDescription;
        private MOBPAXTYPE travelerType;

        public int PaxID
        {
            get { return paxID; }
            set { paxID = value; }
        }
        public string PaxType
        {
            get { return paxType; }
            set { paxType = value; }
        }
        public string TravelerDescription
        {
            get { return travelerDescription; }
            set { travelerDescription = value; }
        }
        
        public MOBPAXTYPE TravelerType
        {
            get { return travelerType; }
            set { travelerType = value; }
        }
    }

    [Serializable]
    //[JsonConverter(typeof(StringEnumConverter))]
    public enum MOBPAXTYPE
    {
        [EnumMember(Value = "Adult")]
        Adult,
        [EnumMember(Value = "Child2To4")]
        Child2To4,
        [EnumMember(Value = "Child5To11")]
        Child5To11,
        [EnumMember(Value = "Child12To17")]
        Child12To17,
        [EnumMember(Value = "InfantLap")]
        InfantLap,
        [EnumMember(Value = "InfantSeat")]
        InfantSeat,
        [EnumMember(Value = "Senior")]
        Senior,
        [EnumMember(Value = "Child12To14")]
        Child12To14 ,
        [EnumMember(Value = "Child15To17")]
        Child15To17 
    }
}