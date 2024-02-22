using System.Collections.Generic;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.Common
{
    public interface ISessionHelperService
    {
        //Get Methods
        Task<T> GetSession<T>(string sessionID, string objectName, List<string> vParams = null, bool isReadOnPrem = false);

        //Save Method
        Task<bool> SaveSession<T>(T data, string sessionID, List<string> validateParams, string objectName = "", int sessionTimeSpanInSecs = 5400, bool saveJsonOnCloudXMLOnPrem = false);

        Task<T> GetSession<T>(string sessionID, string objectName, int temp, List<string> vParams = null);

        Task<bool> SaveSessions<T>(T data, string sessionID, List<string> validateParams, string objectName = "");
    }
}
