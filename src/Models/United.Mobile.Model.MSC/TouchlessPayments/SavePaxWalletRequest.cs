using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.TouchlessPayments
{
    public class SavePaxWalletRequest
    {
        public List<Segments> Segments { get; set; }
        public List<PaxToPay> PaxToPay { get; set; }
        public CreditCard CreditCard { get; set; }
        public string RecordLocator { get; set; }
        public string PointOfSaleCountryCode { get; set; }
        public string ChannelName { get; set; }
        public string IPAddress { get; set; }
        public string PaymentOrigin { get; set; }
        public OptInType OptInType { get; set; }
    }

    public class LookupAndSaveProfileCardRequest : SavePaxWalletRequest
    {
        public bool IsLoyaltyProfileCard { get; set; }
        public string LoyaltyProgramMemberID { get; set; }
    }

    public enum OptInType {
        Checkin,
        Preorder,
        Booking,
        PostCheckin
    }
}
