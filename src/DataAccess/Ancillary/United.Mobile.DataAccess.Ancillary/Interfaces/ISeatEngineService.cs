using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UAWSSeatEngine.SeatMapEngineSoap;

namespace United.Mobile.DataAccess.Product.Interfaces
{
   public interface ISeatEngineService
    {
        Task<GetMerchandizingOffersOutput> GetSeatMapWithFeesFromCSlResponse(SeatMapEngineSoapClient seatMapEngineSoapClient);
    }
}
