using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Common;

namespace United.Mobile.DataAccess.DynamoDB
{
    public class CatalogDynamoDB
    {
        private readonly IConfiguration _configuration;
        private readonly IDynamoDBService _dynamoDBService;
        public CatalogDynamoDB(IConfiguration configuration, IDynamoDBService dynamoDBService)
        {
            _configuration = configuration;
            _dynamoDBService = dynamoDBService;
        }

        public async Task<T> GetCatalogItems<T>(string itemId, string sessionId)
        {
            try
            {
                return await _dynamoDBService.GetRecords<T>(_configuration?.GetValue<string>("DynamoDBTables:uatb_Catalog"), "catalog01", itemId, sessionId);
            }
            catch
            {

            }

            return default;
        }
    }
}
