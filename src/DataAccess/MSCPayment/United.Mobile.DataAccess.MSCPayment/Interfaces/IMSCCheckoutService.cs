using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface IMSCCheckoutService
    {
        Task<(T response, long callDuration)> GetCustomerData<T>(string token, string sessionId, string jsonRequest);
    }
}
