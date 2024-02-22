using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.Common
{
    public interface IVerifyMileagePlusHashpinService
    {
        Task<T> VerifyMileagePlusHashpin<T>(string token, string request, string sessionId);
    }
}
