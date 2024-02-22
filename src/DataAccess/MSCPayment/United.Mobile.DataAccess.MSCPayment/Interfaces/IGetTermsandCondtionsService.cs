using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
   public interface IGetTermsandCondtionsService
    {
        Task<string> GetTermsandCondtionsByPromoCode(string path, string sessionId, string token);

       
    }
}
