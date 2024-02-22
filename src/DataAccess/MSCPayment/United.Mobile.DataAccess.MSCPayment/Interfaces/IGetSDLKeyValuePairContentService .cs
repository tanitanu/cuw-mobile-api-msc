using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface IGetSDLKeyValuePairContentService
    {
        Task<string> GetSDLKeyValuePairContentByPageName(string path, string sessionId, string token);
    }
}
