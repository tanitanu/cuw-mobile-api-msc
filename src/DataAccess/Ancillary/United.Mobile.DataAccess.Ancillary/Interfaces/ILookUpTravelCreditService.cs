using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using United.Definition;

namespace United.Mobile.DataAccess.Product.Interfaces
{
    public interface ILookUpTravelCreditService
    {
        Task<MOBFOPResponse> LookUpTravelCredit(string token, string path, MOBFOPLookUpTravelCreditRequest request, string sessionId);
    }
}
