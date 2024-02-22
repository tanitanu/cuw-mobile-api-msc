using System.Threading.Tasks;

namespace United.Mobile.DataAccess.Product.Interfaces
{
    public interface ICMSContentService
    {
        Task<T> GetSDLContentByGroupName<T>(string token, string action, string request, string sessionId);
        Task<string> GetETCByEmail(string path, string request, string sessionId, string token);
    }
}
