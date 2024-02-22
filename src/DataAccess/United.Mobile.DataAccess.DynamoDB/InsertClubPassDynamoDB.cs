using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Common;

namespace United.Mobile.DataAccess.DynamoDB
{
    public class InsertClubPassDynamoDB
    {
        private readonly IConfiguration _configuration;
        private readonly IDynamoDBService _dynamoDBService;

        public InsertClubPassDynamoDB(IConfiguration configuration
            , IDynamoDBService dynamoDBService)
        {
            _configuration = configuration;
            _dynamoDBService = dynamoDBService;
        }

        public async Task<bool> InsertUnitedClubPassToDB<T>(T data, string key, string sessionID)
        {
            try
            {
                string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("utb_UnitedClubPass");
                string transId = string.IsNullOrEmpty(sessionID) ? "device001" : sessionID;
                return await _dynamoDBService.SaveRecords<T>(tableName, transId, key, data, sessionID);
            }
            catch { }
            return false;
        }
    }
}
