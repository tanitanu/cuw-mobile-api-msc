using System.Collections.Generic;
using System.Threading.Tasks;
using United.Definition;
using United.Mobile.Model.Common;
namespace United.Mobile.MSCCheckOut.Domain
{
    public interface IMSCCheckOutBusiness
    {
        Task<MOBCheckOutResponse> CheckOut(MOBCheckOutRequest request);

        Task<List<MOBItem>> GetMPPINPWDTitleMessages(string titleList);
    }
}
