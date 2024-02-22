using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface ICustomerPreferencesService
    {
        Task<T> GetCustomerPrefernce<T>(string token, string savedUnfinishedBookingActionName, string savedUnfinishedBookingAugumentName, int customerID, string sessionId);
        Task<string> PurgeAnUnfinishedBooking(string token, string action, string sessionId);
    }
}
