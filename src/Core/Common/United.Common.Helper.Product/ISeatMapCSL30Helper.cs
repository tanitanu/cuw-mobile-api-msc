using System.Collections.Generic;
using United.Definition;

namespace United.Common.Helper.Product
{
    public interface ISeatMapCSL30Helper
    {
        System.Threading.Tasks.Task<List<MOBSeatMap>> GetCSL30SeatMapDetail
           (string flow, string sessionId, string destination, string origin, int applicationId,
           string appVersion, string deviceId, bool returnPolarisLegendforSeatMap);
        System.Threading.Tasks.Task<List<MOBSeatMap>> GetSeatMapDetail(string sessionId, string destination, string origin, int applicationId, string appVersion, string deviceId, bool returnPolarisLegendforSeatMap);
    }
}
