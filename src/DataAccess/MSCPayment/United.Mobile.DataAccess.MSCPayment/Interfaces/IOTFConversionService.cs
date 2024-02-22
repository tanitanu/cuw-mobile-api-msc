using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface IOTFConversionService
    {
        Task<string> OTFConversionByPnr(string path, string request, string sessionId, string token);
    }
}
