using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MSCPayment.Interfaces
{
    public interface IReferencedataService
    {

        Task<T> GetDataPostHttpAsyncWithOptions<T>(string path, string token, string sessionId, string request);

        Task<T> GetDataGetAsync<T>(string actionName, string token, string sessionId);
    }
}
