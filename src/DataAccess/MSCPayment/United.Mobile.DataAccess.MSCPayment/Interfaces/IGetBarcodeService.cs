using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface IGetBarcodeService
    {
        public Task<string> GetClubPassCode(string path, string request, string sessionId, string token);
    }
}
