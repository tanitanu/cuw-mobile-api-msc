using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Common;
using United.Mobile.Model.Common;
using United.Mobile.Model.DynamoDb.Common;

namespace United.Mobile.DataAccess.DynamoDB
{
    public class AirportDynamoDB
    {
        private readonly IConfiguration _configuration;
        private readonly IDynamoDBService _dynamoDBService;
        private string tableName = string.Empty;

        public AirportDynamoDB(IConfiguration configuration
            , IDynamoDBService dynamoDBService)
        {
            _configuration = configuration;
            _dynamoDBService = dynamoDBService;
            tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("utb_Airport");
            if (string.IsNullOrEmpty(tableName))
                tableName = "utb_Airport";
        }


        public async Task<string> GetAirportName(string airportCode, string sessionId)
        {
            var response = await _dynamoDBService.GetRecords<DisplayAirportDetails>(tableName, "Airport-" + airportCode.ToUpper(), airportCode.ToUpper(), sessionId).ConfigureAwait(false);
            return string.IsNullOrEmpty(response?.AirportNameMobile) ? airportCode : response?.AirportNameMobile;
        }

        public async Task<(string airportName, string cityName)> GetAirportCityName(string airportCode, string sessionId)
        {
            string airportName;
            string cityName;

            #region
            //Database database = DatabaseFactory.CreateDatabase("ConnectionString - iPhone");
            //DbCommand dbCommand = (DbCommand)database.GetStoredProcCommand("usp_Select_AirportName");
            //database.AddInParameter(dbCommand, "@AirportCode", DbType.String, airportCode);

            //using (IDataReader dataReader = database.ExecuteReader(dbCommand))
            //{
            //    while (dataReader.Read())
            //    {
            //        airportName = dataReader["AirportName"].ToString();
            //        cityName = dataReader["CityName"].ToString();
            //    }
            //}
            #endregion
            try
            {
                var response = await _dynamoDBService.GetRecords<DisplayAirportDetails>(tableName, "Airport-" + airportCode.ToUpper(), airportCode.ToUpper(), sessionId).ConfigureAwait(false);
                if (response != null)
                {
                    airportName = response.AirportNameMobile;
                    cityName = response.CityName;
                    return (airportName, cityName);
                }
            }
            catch
            {

            }

            return default;
        }
    }
}
