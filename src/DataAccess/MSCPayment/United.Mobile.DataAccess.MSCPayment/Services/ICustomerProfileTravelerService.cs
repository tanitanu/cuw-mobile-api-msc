using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Services
{
    public interface ICustomerProfileTravelerService
    {
        Task<string> GetProfileTravelerInfo(string token, string sessionId, string mpNumber);
    }
}
