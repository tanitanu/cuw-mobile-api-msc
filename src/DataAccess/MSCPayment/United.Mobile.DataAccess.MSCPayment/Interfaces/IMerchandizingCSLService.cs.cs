using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface IMerchandizingCSLService
    {
        Task<string> DeclineTPIOffer(string token, string request);
    }
}
