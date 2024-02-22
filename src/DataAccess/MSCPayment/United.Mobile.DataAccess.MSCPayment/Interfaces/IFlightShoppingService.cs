using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface IFlightShoppingService
    {
        Task<string> GetProducts(string token, string request, string sessionId);
        
        Task<string> GetCartInformation(string token, string action, string request, string sessionId);

    }
}
