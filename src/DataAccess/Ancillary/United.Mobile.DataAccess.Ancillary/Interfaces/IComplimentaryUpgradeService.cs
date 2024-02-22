﻿using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.Model.DynamoDb.Common;

namespace United.Mobile.DataAccess.Product.Interfaces
{
    public interface IComplimentaryUpgradeService
    {
        Task<List<CabinBrand>> GetComplimentaryUpgradeOfferedFlagByCabinCount(string Origin, string destination, int numberOfCabins, string sessionId, string transactionId);
    }
}
