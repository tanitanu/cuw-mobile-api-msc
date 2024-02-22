using System.Threading.Tasks;

namespace United.Mobile.DataAccess.Product.Interfaces
{
    public interface ISeatMapCSL30Service
    {
        Task<string> GetSeatMapDeatils(string token, string sessionId, string request, string channelId, string channelName, string path);
    }
}
