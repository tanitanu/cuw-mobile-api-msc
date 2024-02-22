using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MSC
{
    public class CslMemberProfile { 
        public CslAccountStatus Data { get; set; }
    }
    public class CslAccountStatus
    {
        public long CustomerId { get; set; }
        public List<Balance> Balances { get; set; }
    }
    public class Balance
    {
        public decimal Amount { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Currency { get; set; }
    }
}
