using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Common;

namespace United.Mobile.DataAccess.DynamoDB
{
    public class InflightDynamoDB
    {
        private readonly IConfiguration _configuration;
        private readonly IDynamoDBService _dynamoDBService;

        public InflightDynamoDB(IConfiguration configuration
            , IDynamoDBService dynamoDBService)
        {
            _configuration = configuration;
            _dynamoDBService = dynamoDBService;
        }


        public async Task<T> GetInflightContent<T>(string key)
        {
            return await _dynamoDBService.GetRecords<T>(_configuration?.GetValue<string>("DynamoDBTables:cachecontentKey"), "cache_content", key,"");
        }
    }
}
