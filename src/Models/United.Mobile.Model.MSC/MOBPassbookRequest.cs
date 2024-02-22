using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable]
    public class MOBPassbookRequest
    {
        public string gate;
        public string gateChangeMsg;
        public string origCode;
        public string origCity;
        public string destCode;
        public string destCity;
        public string paxName;
        public string premierStatus;
        public string boardTime;
        public string boardTimeChangeMsg;
        public string flightNum;
        public string flightNumChangeMsg;
        public string seat;
        public string seatChangeMsg;
        public string group;
        public string groupChangeMsg;
        public string confNum;
        public string confNumChgMsg;
        public string seqNum;
        public string cabin;
        public string cabinChangeMsg;
        public string mileagePlus;
        public string flightDate;
        public string flightDateChangeMsg;
        public string departTime;
        public string departTimeChangeMsg;
        public string arriveTime;
        public string arriveTimeChangeMsg;
        public string terms;
        public string barcodeMessage;
        public string serialNumber;
        public string transactionId;
        public string exitRowSeat;
        public string operatedBy;
        public string addedToSBY;
        public string departureDate;
        public string scheduledDepartGMT;
        public string isTSAPreCheck;
        public string modID;
        public string elfDescription;
        public bool isIBELite; //Added by Nizam - 07/18/2018 - Fix for IBELite color change on mBP update
    }
}
