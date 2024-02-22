using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.FormofPayment;

namespace United.Common.Helper
{
    public interface IMoneyPlusMilesUtility
    {
        Task GetMoneyPlusMilesOptionsForFinalRTIScreen(MOBRegisterSeatsRequest request, MOBBookingRegisterSeatsResponse response, United.Persist.Definition.Shopping.Session session, MOBShoppingCart shoppingCart);
    }

}
