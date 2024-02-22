using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface IProvisionService
    {
        public Task<string> CSL_PartnerProvisionCall(string token, string path, string request);
    }
}
