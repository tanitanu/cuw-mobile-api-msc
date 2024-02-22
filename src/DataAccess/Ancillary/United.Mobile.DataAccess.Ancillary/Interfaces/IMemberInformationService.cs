using System.Collections.Generic;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.Product.Interfaces
{
    public interface IMemberInformationService
    {
        Task<string> GetMemberInformation(string token, string loyaltyId, bool returnMilesBalanceOnly);

    }
}
