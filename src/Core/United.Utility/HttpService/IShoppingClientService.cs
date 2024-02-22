using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace United.Utility.HttpService
{
    public interface IShoppingClientService
    {
        public Task<T> PostHttpAsyncWithOptions<T>(string token, string sessionId, string action, object request, string contentType = "application/json");
    }
}
