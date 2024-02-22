using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Common;

namespace United.Mobile.DataAccess.DynamoDB
{
    public class SeatMapDynamoDB
    {
        private readonly IConfiguration _configuration;
        private readonly IDynamoDBService _dynamoDBService;

        public SeatMapDynamoDB(IConfiguration configuration
            , IDynamoDBService dynamoDBService)
        {
            _configuration = configuration;
            _dynamoDBService = dynamoDBService;
        }


        public async Task<T> GetBillingAddressCountries<T>(string key, string sessionId)
        {
            #region
            //Database database = DatabaseFactory.CreateDatabase("ConnectionString - iPhone");
            //DbCommand dbCommand = (DbCommand)database.GetStoredProcCommand("uasp_Select_BillingCountryList");
            //using (IDataReader dataReader = database.ExecuteReader(dbCommand))
            //{
            //    while (dataReader.Read())
            //    {
            //        billingCountries.Add(new MOBCPBillingCountry
            //        {
            //            CountryName = Convert.ToString(dataReader["CountryName"]),
            //            CountryCode = Convert.ToString(dataReader["CountryCode"]),
            //            Id = Convert.ToString(dataReader["BillingCountryOrder"]),
            //            IsStateRequired = Convert.ToBoolean(dataReader["IsStateRequired"]),
            //            IsZipRequired = Convert.ToBoolean(dataReader["IsZipPostalRequired"]),
            //        });
            //    }
            //}

            #endregion
            return await _dynamoDBService.GetRecords<T>(_configuration?.GetValue<string>("DynamoDBTables:cachecontentKey"), "cache_content", key, sessionId);
        }
    }
}
