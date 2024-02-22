using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface IGetCCEContentService
    {

        Task<string> GetCCEContent(string token, string action, string request, string sessionId);
    }
}
